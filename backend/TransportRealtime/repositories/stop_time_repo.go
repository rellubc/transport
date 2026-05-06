package repositories

import (
	models "TransportRealtime/models/static"
	"context"
	"fmt"
	"log"
	"strings"

	"github.com/jackc/pgx/v5/pgxpool"
)

type StopTimeRepository struct {
	DB *pgxpool.Pool
}

func NewStopTimeRepository(db *pgxpool.Pool) *StopTimeRepository {
	return &StopTimeRepository{DB: db}
}

func (r *StopTimeRepository) WarmCache() error {
	prewarmQueries := []string{
		`SELECT pg_prewarm('active_stop_departures')`,
		`SELECT pg_prewarm('idx_asd_parent_stop_time')`,
		`SELECT pg_prewarm('idx_asd_unique')`,
	}

	for _, q := range prewarmQueries {
		if _, err := r.DB.Exec(context.Background(), q); err != nil {
			return fmt.Errorf("prewarm query failed: %w", err)
		}
	}

	log.Println("cache warmup complete")
	return nil
}

func (r *StopTimeRepository) GetStaticStopTimes(stopId string, tripId string) ([]models.StaticStopTime, error) {
	baseQuery := "SELECT st.trip_id, st.arrival_time, st.departure_time, st.stop_id, st.stop_sequence, st.pickup_type, st.drop_off_type, st.shape_dist_travelled, st.timepoint, st.stop_note, st.route_type, t.trip_headsign AS stop_headsign FROM stop_times st JOIN trips t ON st.trip_id = t.trip_id"
	args := []any{}
	conditions := []string{}

	if stopId != "" {
		conditions = append(conditions, fmt.Sprintf("st.stop_id = $%d", len(args)+1))
		args = append(args, stopId)
	}

	if tripId != "" {
		conditions = append(conditions, fmt.Sprintf("st.trip_id = $%d", len(args)+1))
		args = append(args, tripId)
	}

	if len(conditions) > 0 {
		baseQuery += " WHERE " + strings.Join(conditions, " AND ")
	}

	rows, err := r.DB.Query(context.Background(), baseQuery, args...)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var stopTimes []models.StaticStopTime
	for rows.Next() {
		var stopTime models.StaticStopTime
		err := rows.Scan(
			&stopTime.TripId,
			&stopTime.ArrivalTime,
			&stopTime.DepartureTime,
			&stopTime.StopId,
			&stopTime.StopSequence,
			&stopTime.PickupType,
			&stopTime.DropOffType,
			&stopTime.ShapeDistTravelled,
			&stopTime.Timepoint,
			&stopTime.StopNote,
			&stopTime.RouteType,
			&stopTime.StopHeadsign,
		)

		if err != nil {
			return nil, err
		}

		stopTimes = append(stopTimes, stopTime)
	}

	return stopTimes, nil
}

func (r *StopTimeRepository) GetRealtimeStopTimes(stopId string, tripId string) ([]models.RealtimeStopTime, error) {
	baseQuery := `
		SELECT
			st.stop_id,
			s.stop_name,
			st.stop_sequence,
			t.trip_headsign,
			st.arrival_time,
			stu.stop_arrival_delay,
			st.departure_time,
			stu.stop_departure_delay,
			COALESCE(progress, 'skipped') AS progress,
			COALESCE(
				json_agg(
					json_build_object(
						'positionInConsist', c.position_in_consist,
						'occupancyStatus', c.occupancy_status
					)
					ORDER BY c.position_in_consist ASC
				) FILTER (WHERE c.trip_id IS NOT NULL),
				'[]'
			) AS consist
		FROM stop_times st
		LEFT JOIN stop_time_updates_with_progress stu
			ON st.stop_id = stu.stop_id
			AND st.trip_id = stu.trip_id
		JOIN stops s
			ON st.stop_id = s.stop_id
			AND st.route_type = s.route_type
		JOIN trips t
			ON st.trip_id = t.trip_id
		LEFT JOIN LATERAL (
			SELECT DISTINCT ON (position_in_consist) position_in_consist, occupancy_status, trip_id
			FROM consist
			WHERE trip_id = st.trip_id 
				AND EXTRACT(EPOCH FROM timestamp AT TIME ZONE 'Australia/Sydney')::integer %% 86400 <= (st.departure_time + COALESCE(stu.stop_departure_delay, 0))
			ORDER BY position_in_consist, timestamp DESC
			LIMIT CASE 
				WHEN st.route_type = 401 THEN 6
				WHEN st.route_type = 900 THEN 2
				ELSE 8
			END
		) c ON true
		%s
		GROUP BY
			st.stop_id,
			s.stop_name,
			st.stop_sequence,
			t.trip_headsign,
			st.arrival_time,
			stu.stop_arrival_delay,
			st.departure_time,
			stu.stop_departure_delay,
			stu.progress
		ORDER BY st.stop_sequence
	`
	args := []any{}
	conditions := []string{}

	if stopId != "" {
		conditions = append(conditions, fmt.Sprintf("st.stop_id = $%d", len(args)+1))
		args = append(args, stopId)
	}

	if tripId != "" {
		conditions = append(conditions, fmt.Sprintf("st.trip_id = $%d", len(args)+1))
		args = append(args, tripId)
	}

	whereClause := ""
	if len(conditions) > 0 {
		whereClause += "WHERE " + strings.Join(conditions, " AND ")
	}

	query := fmt.Sprintf(baseQuery, whereClause)

	rows, err := r.DB.Query(context.Background(), query, args...)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var sts []models.RealtimeStopTime
	for rows.Next() {
		var st models.RealtimeStopTime
		err := rows.Scan(
			&st.StopId,
			&st.StopName,
			&st.StopSequence,
			&st.StopHeadsign,
			&st.ArrivalTime,
			&st.ArrivalDelay,
			&st.DepartureTime,
			&st.DepartureDelay,
			&st.Progress,
			&st.Consist,
		)

		log.Println(err)

		if err != nil {
			return nil, err
		}

		if (st.ArrivalDelay == nil) && (st.DepartureDelay == nil) {
			st.Status = "skipped"
		} else {
			st.Status = "stop"
		}

		sts = append(sts, st)
	}

	return sts, nil
}

func (r *StopTimeRepository) GetStopStaticStopTimes(stopId string) ([]models.StaticStopTime, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM static_stop_times WHERE stop_id = $1", stopId)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	return []models.StaticStopTime{}, nil
}

func (r *StopTimeRepository) GetStopStopTimes(stopId string, direction string, time int) ([]models.StopRealtimeStopTime, error) {
	var candidatesFilter string
	switch direction {
	case "prev":
		candidatesFilter = ""
	case "next":
		candidatesFilter = ""
	default:
		candidatesFilter = "AND st.departure_time >= (SELECT now_sec FROM now_sydney) - 3600"
	}

	baseQuery := `
		WITH now_sydney AS MATERIALIZED (
			SELECT
				(NOW() AT TIME ZONE 'Australia/Sydney')::date AS now_date,
				EXTRACT(DOW FROM NOW() AT TIME ZONE 'Australia/Sydney')::int AS now_dow,
				EXTRACT(EPOCH FROM (NOW() AT TIME ZONE 'Australia/Sydney')::time)::int AS now_sec
		),
		active_services AS MATERIALIZED (
			SELECT c.service_id
			FROM calendars c
			CROSS JOIN now_sydney ns
			WHERE c.start_date <= ns.now_date
				AND c.end_date   >= ns.now_date
				AND CASE ns.now_dow
					WHEN 0 THEN c.sunday
					WHEN 1 THEN c.monday
					WHEN 2 THEN c.tuesday
					WHEN 3 THEN c.wednesday
					WHEN 4 THEN c.thursday
					WHEN 5 THEN c.friday
					WHEN 6 THEN c.saturday
				END
		),
		active_trips AS MATERIALIZED (
			SELECT t.trip_id, t.trip_headsign, t.service_id, t.route_id, r.route_short_name, r.route_type, r.route_colour
			FROM trips t
			JOIN active_services ac ON ac.service_id = t.service_id
			JOIN routes r ON t.route_id = r.route_id
		),
		candidates AS MATERIALIZED (
			SELECT DISTINCT ON (t.trip_id, st.stop_id, st.arrival_time)
				t.trip_id,
				COALESCE(NULLIF(st.stop_headsign, ''), t.trip_headsign, 'No headsign') AS trip_headsign,
				t.service_id,
				t.route_id,
				t.route_short_name,
        		t.route_type,
        		t.route_colour,
				st.stop_id,
				s.stop_name,
				st.arrival_time,
				stu.stop_arrival_delay,
				st.departure_time,
				stu.stop_departure_delay,
				st.arrival_time + COALESCE(stu.stop_arrival_delay, 0) AS effective_arrival_time,
				st.departure_time + COALESCE(stu.stop_departure_delay, 0) AS effective_departure_time,
				st.pickup_type,
				st.drop_off_type,
				CASE
					WHEN st.pickup_type = 1 AND st.drop_off_type = 1 THEN 'pass'
                    WHEN (st.pickup_type = 1 OR (st.departure_time - st.arrival_time >= 300 AND s.stop_name ILIKE '%' || t.trip_headsign || '%')) THEN 'terminate'
					WHEN st.drop_off_type = 1 THEN 'depart'
					ELSE 'stop'
				END AS stop_type,
				(stu.stop_departure_delay IS NOT NULL) AS is_realtime
			FROM stops s
			JOIN stop_times st
				ON st.stop_id = s.stop_id ` + candidatesFilter + `
			JOIN active_trips t ON t.trip_id = st.trip_id
			LEFT JOIN LATERAL (
				SELECT stop_arrival_delay, stop_departure_delay
				FROM stop_time_updates
				WHERE trip_id = st.trip_id AND stop_id = st.stop_id
				ORDER BY timestamp DESC
				LIMIT 1
			) stu ON true
			WHERE s.stop_id = $1 OR s.stop_parent_station = $1
		),
		depart_keys AS MATERIALIZED (
			SELECT DISTINCT stop_id, departure_time
			FROM candidates
			WHERE stop_type = 'depart'
		),
		paired AS (
			SELECT
				c.*,
				CASE
					WHEN (c.stop_type = 'terminate' OR c.stop_name ILIKE '%' || c.trip_headsign || '%') THEN c.effective_arrival_time
					ELSE c.effective_departure_time
				END AS display_time,
				(dk.stop_id IS NOT NULL AND (dk.departure_time - c.arrival_time) <= 300) AS has_continuation
			FROM candidates c
			LEFT JOIN depart_keys dk
				ON dk.stop_id = c.stop_id
				AND dk.departure_time = c.departure_time + 1
		)
	`

	args := []any{stopId}
	var query string

	switch direction {
	case "next":
		query = baseQuery +
			`SELECT * FROM paired
			 WHERE display_time > $2 AND NOT has_continuation AND stop_type != 'pass' AND route_id NOT LIKE 'RTTA%'
			 ORDER BY display_time ASC, trip_id ASC
			 LIMIT 20`
		args = append(args, time)

	case "prev":
		query = baseQuery +
			`SELECT * FROM (
				SELECT * FROM paired
				WHERE display_time < $2 AND NOT has_continuation AND stop_type != 'pass' AND route_id NOT LIKE 'RTTA%'
				ORDER BY display_time DESC, trip_id DESC
				LIMIT 20
			) sub
			ORDER BY effective_departure_time ASC, trip_id ASC`
		args = append(args, time)

	default:
		query = baseQuery +
			`SELECT * FROM paired
			 WHERE display_time >= (SELECT now_sec FROM now_sydney) AND NOT has_continuation AND stop_type != 'pass' AND route_id NOT LIKE 'RTTA%'
			 ORDER BY display_time ASC, trip_id ASC
			 LIMIT 20`
	}

	// log.Println(query, args)

	rows, err := r.DB.Query(context.Background(), query, args...)
	if err != nil {
		return nil, fmt.Errorf("GetStopRealtimeStopTimes querying failed: %w", err)
	}
	defer rows.Close()

	var sts = []models.StopRealtimeStopTime{}
	for rows.Next() {
		var st models.StopRealtimeStopTime
		err := rows.Scan(
			&st.TripId,
			&st.TripHeadsign,
			&st.ServiceId,
			&st.RouteId,
			&st.RouteShortName,
			&st.RouteType,
			&st.RouteColour,
			&st.StopId,
			&st.StopName,
			&st.ArrivalTime,
			&st.ArrivalDelay,
			&st.DepartureTime,
			&st.DepartureDelay,
			&st.EffectiveArrivalTime,
			&st.EffectiveDepartureTime,
			&st.PickupType,
			&st.DropOffType,
			&st.StopType,
			&st.IsRealtime,
			&st.DisplayTime,
			&st.HasContinuation,
		)

		if err != nil {
			return nil, fmt.Errorf("GetStopRealtimeStopTimes scanning failed: %w", err)
		}

		sts = append(sts, st)
	}

	return sts, nil
}

func (r *StopTimeRepository) GetTripStopTimes(tripId string, vehicleLon float64, vehicleLat float64) ([]models.TripRealtimeStopTime, error) {
	query := `
		WITH stop_data AS (
			SELECT 
				COALESCE(NULLIF(st.stop_headsign, ''), t.trip_headsign, 'No headsign') AS trip_headsign,
				t.shape_id,
				t.trip_direction_id,
				r.route_short_name,
				r.route_type,
				r.route_colour,
				r.route_id,
				st.stop_sequence,
				s.stop_id,
				s.stop_lat,
				s.stop_lon,
				s.stop_name,
				stu.stop_arrival_delay,
				stu.stop_departure_delay,
				st.arrival_time + COALESCE(stu.stop_arrival_delay, 0) AS effective_arrival_time,
				st.departure_time + COALESCE(stu.stop_departure_delay, 0) AS effective_departure_time,
				CASE
					WHEN st.pickup_type = 1 AND st.drop_off_type = 1 THEN 'pass'
					WHEN (st.pickup_type = 1 OR (st.departure_time - st.arrival_time >= 300 AND s.stop_name ILIKE '%' || t.trip_headsign || '%')) THEN 'terminate'
					WHEN st.drop_off_type = 1 THEN 'depart'
					ELSE 'stop'
				END AS stop_type,
				CASE
					WHEN st.pickup_type = 1 AND st.drop_off_type = 1 THEN 'skipped'::text
					WHEN EXTRACT(epoch FROM (now() AT TIME ZONE 'Australia/Sydney'::text))::integer % 86400 > st.departure_time + COALESCE(stu.stop_departure_delay, (0)::bigint) THEN 'passed'::text
					ELSE 'not_passed'::text
				END AS progress,
				(stu.stop_departure_delay IS NOT NULL OR stu.stop_arrival_delay IS NOT NULL) AS is_realtime,
				COALESCE(
					json_agg(
						json_build_object(
							'positionInConsist', c.position_in_consist,
							'occupancyStatus', c.occupancy_status
						)
						ORDER BY c.position_in_consist ASC
					) FILTER (WHERE c.trip_id IS NOT NULL),
					'[]'
				) AS consist
			FROM trips t
			JOIN stop_times st ON t.trip_id = st.trip_id
			LEFT JOIN stop_time_updates stu ON st.stop_id = stu.stop_id AND st.trip_id = stu.trip_id
			JOIN stops s ON st.stop_id = s.stop_id
			JOIN routes r ON t.route_id = r.route_id
			LEFT JOIN LATERAL (
				SELECT DISTINCT ON (timestamp, position_in_consist) timestamp, position_in_consist, occupancy_status, trip_id
				FROM consist
				WHERE trip_id = st.trip_id 
					AND EXTRACT(EPOCH FROM timestamp AT TIME ZONE 'Australia/Sydney')::integer % 86400 <= (st.departure_time + COALESCE(stu.stop_departure_delay, 0))
				ORDER BY timestamp DESC, position_in_consist
				LIMIT CASE 
					WHEN st.route_type = 401 THEN 6
					WHEN st.route_type = 900 THEN 2
					ELSE 8
				END
			) c ON true
			WHERE t.trip_id = $1
				AND (st.pickup_type = 0 OR st.drop_off_type = 0)
			GROUP BY
				st.stop_headsign,
				t.trip_headsign,
				t.shape_id,
				t.trip_direction_id,
				r.route_short_name,
				r.route_type,
				r.route_colour,
				r.route_id,
				st.stop_sequence,
				s.stop_id,
				s.stop_lat,
				s.stop_lon,
				s.stop_name,
				st.arrival_time,
				st.departure_time,
				stu.stop_arrival_delay,
				stu.stop_departure_delay,
				st.arrival_time + COALESCE(stu.stop_arrival_delay, 0),
				st.departure_time + COALESCE(stu.stop_departure_delay, 0),
				st.pickup_type,
				st.drop_off_type,
				is_realtime
			ORDER BY st.stop_sequence
		),
		stop_window AS (
			SELECT
				*,
				CASE
					WHEN stop_data.stop_type = 'terminate' OR stop_data.stop_name ILIKE '%' || stop_data.trip_headsign || '%'
					THEN stop_data.effective_arrival_time
					ELSE stop_data.effective_departure_time
				END AS display_time
			FROM stop_data
		),
		prev_stop AS (
			SELECT * FROM stop_window
			WHERE progress = 'passed'
			ORDER BY stop_sequence DESC
			LIMIT 1
		),
		next_stop AS (
			SELECT * FROM stop_window
			WHERE progress = 'not_passed'
			ORDER BY stop_sequence ASC
			LIMIT 1
		),
		prev_shape_point AS (
			SELECT sh.*
			FROM prev_stop ps
			JOIN shapes sh ON sh.shape_id = (
				SELECT sh2.shape_id
				FROM shapes sh2
				WHERE sh2.shape_id LIKE 'ROUTE_' || ps.route_short_name || '%'
				ORDER BY point(sh2.shape_pt_lon, sh2.shape_pt_lat) <-> point(ps.stop_lon, ps.stop_lat)
				LIMIT 1
			)
			ORDER BY point(sh.shape_pt_lon, sh.shape_pt_lat) <-> point(ps.stop_lon, ps.stop_lat)
			LIMIT 1
		),
		next_shape_point AS (
			SELECT sh.*
			FROM next_stop ns
			JOIN shapes sh ON sh.shape_id = (
				SELECT sh2.shape_id
				FROM shapes sh2
				WHERE sh2.shape_id LIKE 'ROUTE_' || ns.route_short_name || '%'
				ORDER BY point(sh2.shape_pt_lon, sh2.shape_pt_lat) <-> point(ns.stop_lon, ns.stop_lat)
				LIMIT 1
			)
			ORDER BY point(sh.shape_pt_lon, sh.shape_pt_lat) <-> point(ns.stop_lon, ns.stop_lat)
			LIMIT 1
		),
		current_shape_point AS (
			SELECT sh.*
			FROM next_stop ns
			JOIN shapes sh ON sh.shape_id = (
				SELECT sh2.shape_id
				FROM shapes sh2
				WHERE sh2.shape_id LIKE 'ROUTE_' || ns.route_short_name || '%'
				ORDER BY point(sh2.shape_pt_lon, sh2.shape_pt_lat) <-> point($2, $3)
				LIMIT 1
			)
			ORDER BY point(sh.shape_pt_lon, sh.shape_pt_lat) <-> point($2, $3)
			LIMIT 1
		),
		progress AS (
			SELECT
				ROUND(
					ABS(csp.shape_pt_sequence - psp.shape_pt_sequence)::numeric
					/ NULLIF(ABS(nsp.shape_pt_sequence - psp.shape_pt_sequence), 0)
					* 100
				, 2) AS progress
			FROM prev_shape_point psp, current_shape_point csp, next_shape_point nsp
		)
		SELECT 
			sw.trip_headsign,
			sw.route_short_name,
			sw.route_type,
			sw.route_colour,
			sw.stop_id,
			sw.stop_name,
			sw.stop_arrival_delay,
			sw.stop_departure_delay,
			sw.effective_arrival_time,
			sw.effective_departure_time,
			sw.stop_type,
			sw.progress,
			sw.is_realtime,
			sw.consist,
			sw.display_time,
			CASE 
				WHEN sw.route_id LIKE 'RTTA%' THEN 0
				WHEN sw.progress = 'passed' THEN 100
				WHEN NOT EXISTS (SELECT 1 FROM prev_stop) THEN 0
				WHEN ns.stop_id != sw.stop_id THEN 0
				ELSE p.progress
			END as stop_progress
		FROM stop_window sw, next_stop ns, progress p
		;
	`

	args := []any{tripId, vehicleLon, vehicleLat}

	log.Println(query, args)

	rows, err := r.DB.Query(context.Background(), query, args...)
	if err != nil {
		return nil, fmt.Errorf("GetTripRealtimeStopTimes querying failed: %w", err)
	}
	defer rows.Close()

	var sts = []models.TripRealtimeStopTime{}
	for rows.Next() {
		var st models.TripRealtimeStopTime
		err := rows.Scan(
			&st.TripHeadsign,
			&st.RouteShortName,
			&st.RouteType,
			&st.RouteColour,
			&st.StopId,
			&st.StopName,
			&st.ArrivalDelay,
			&st.DepartureDelay,
			&st.EffectiveArrivalTime,
			&st.EffectiveDepartureTime,
			&st.StopType,
			&st.Progress,
			&st.IsRealtime,
			&st.Consist,
			&st.DisplayTime,
			&st.StopProgress,
		)

		if err != nil {
			return nil, fmt.Errorf("GetTripRealtimeStopTimes scanning failed: %w", err)
		}

		sts = append(sts, st)
	}

	return sts, nil
}

// WITH stop_data AS (
// 	SELECT
// 		COALESCE(NULLIF(st.stop_headsign, ''), t.trip_headsign, 'No headsign') AS trip_headsign,
// 		r.route_short_name,
// 		r.route_type,
// 		r.route_colour,
// 		s.stop_id,
// 		s.stop_name,
// 		stu.stop_arrival_delay,
// 		stu.stop_departure_delay,
// 		st.arrival_time + COALESCE(stu.stop_arrival_delay, 0) AS effective_arrival_time,
// 		st.departure_time + COALESCE(stu.stop_departure_delay, 0) AS effective_departure_time,
// 		CASE
// 			WHEN st.pickup_type = 1 AND st.drop_off_type = 1 THEN 'pass'
// 			WHEN (st.pickup_type = 1 OR (st.departure_time - st.arrival_time >= 300 AND s.stop_name ILIKE '%' || t.trip_headsign || '%')) THEN 'terminate'
// 			WHEN st.drop_off_type = 1 THEN 'depart'
// 			ELSE 'stop'
// 		END AS stop_type,
// 		CASE
// 			WHEN st.pickup_type = 1 AND st.drop_off_type = 1 THEN 'skipped'::text
// 			WHEN EXTRACT(epoch FROM (now() AT TIME ZONE 'Australia/Sydney'::text))::integer % 86400 > st.departure_time + COALESCE(stu.stop_departure_delay, (0)::bigint) THEN 'passed'::text
// 			ELSE 'not_passed'::text
// 		END AS progress,
// 		(stu.stop_departure_delay IS NOT NULL OR stu.stop_arrival_delay IS NOT NULL) AS is_realtime,
// 		COALESCE(
// 			json_agg(
// 				json_build_object(
// 					'positionInConsist', c.position_in_consist,
// 					'occupancyStatus', c.occupancy_status
// 				)
// 				ORDER BY c.position_in_consist ASC
// 			) FILTER (WHERE c.trip_id IS NOT NULL),
// 			'[]'
// 		) AS consist
// 	FROM trips t
// 	JOIN stop_times st ON t.trip_id = st.trip_id
// 	LEFT JOIN stop_time_updates stu ON st.stop_id = stu.stop_id AND st.trip_id = stu.trip_id
// 	JOIN stops s ON st.stop_id = s.stop_id
//     JOIN routes r ON t.route_id = r.route_id
// 	LEFT JOIN LATERAL (
// 		SELECT DISTINCT ON (position_in_consist) position_in_consist, occupancy_status, trip_id
// 		FROM consist
// 		WHERE trip_id = st.trip_id
// 			AND EXTRACT(EPOCH FROM timestamp AT TIME ZONE 'Australia/Sydney')::integer % 86400 <= (st.departure_time + COALESCE(stu.stop_departure_delay, 0))
// 		ORDER BY position_in_consist, timestamp DESC
// 		LIMIT CASE
// 			WHEN st.route_type = 401 THEN 6
// 			WHEN st.route_type = 900 THEN 2
// 			ELSE 8
// 		END
// 	) c ON true
// 	WHERE t.trip_id = $1
// 		AND (st.pickup_type = 0 OR st.drop_off_type = 0)
// 	GROUP BY
// 		st.stop_headsign,
// 		t.trip_headsign,
//         r.route_short_name,
//         r.route_type,
//         r.route_colour,
// 		st.stop_sequence,
// 		s.stop_id,
// 		s.stop_name,
// 		st.arrival_time,
// 		st.departure_time,
// 		stu.stop_arrival_delay,
// 		stu.stop_departure_delay,
// 		st.arrival_time + COALESCE(stu.stop_arrival_delay, 0),
// 		st.departure_time + COALESCE(stu.stop_departure_delay, 0),
// 		st.pickup_type,
// 		st.drop_off_type,
// 		is_realtime
// 	ORDER BY st.stop_sequence
// )
// SELECT
// 	*,
// 	CASE
// 		WHEN stop_data.stop_type = 'terminate' OR stop_data.stop_name ILIKE '%' || stop_data.trip_headsign || '%'
// 		THEN stop_data.effective_arrival_time
// 		ELSE stop_data.effective_departure_time
// 	END AS display_time
// FROM stop_data
// ;
