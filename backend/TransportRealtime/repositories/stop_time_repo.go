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
			SELECT t.trip_id, t.trip_headsign, t.service_id, t.route_id
			FROM trips t
			JOIN active_services ac ON ac.service_id = t.service_id
		),
		target_stops AS MATERIALIZED (
			SELECT stop_id, stop_name
			FROM stops
			WHERE stop_id = $1 OR stop_parent_station = $1
		),
		candidates AS (
			SELECT
				t.trip_id,
				COALESCE(NULLIF(st.stop_headsign, ''), t.trip_headsign, 'No headsign') AS trip_headsign,
				t.service_id,
				t.route_id,
				st.stop_id,
				s.stop_name,
				st.arrival_time,
				stu.stop_arrival_delay,
				st.departure_time,
				stu.stop_departure_delay,
				st.arrival_time  + COALESCE(stu.stop_arrival_delay,   0) AS effective_arrival_time,
				st.departure_time + COALESCE(stu.stop_departure_delay, 0) AS effective_departure_time,
				st.pickup_type,
				st.drop_off_type,
				CASE
						WHEN st.pickup_type = 1 AND st.drop_off_type = 1 THEN 'pass'
						WHEN st.pickup_type = 1 THEN 'terminate'
						WHEN st.drop_off_type = 1 THEN 'depart'
						ELSE 'stop'
				END AS stop_type,
				(stu.stop_departure_delay IS NOT NULL) AS is_realtime
			FROM target_stops s
			JOIN stop_times st
				ON  st.stop_id = s.stop_id
			JOIN active_trips t ON t.trip_id = st.trip_id
			LEFT JOIN LATERAL (
				SELECT stop_arrival_delay, stop_departure_delay
				FROM stop_time_updates
				WHERE trip_id = st.trip_id AND stop_id = st.stop_id
				ORDER BY timestamp DESC
				LIMIT 1
			) stu ON true
		)
    `

	args := []any{stopId}

	var query string

	switch direction {
	case "next":
		query = baseQuery +
			`SELECT * FROM candidates
			 WHERE effective_departure_time > $2
			 ORDER BY effective_departure_time ASC, trip_id ASC
			 LIMIT 20`
		args = append(args, time)

	case "prev":
		query = baseQuery +
			`SELECT * FROM (
				SELECT * FROM candidates
				WHERE effective_departure_time < $2
				ORDER BY effective_departure_time DESC, trip_id DESC
				LIMIT 20
			 ) sub
			 ORDER BY effective_departure_time ASC, trip_id ASC`
		args = append(args, time)

	default:
		query = baseQuery +
			`SELECT * FROM candidates
			WHERE effective_departure_time >= (SELECT now_sec FROM now_sydney)
			ORDER BY effective_departure_time ASC, trip_id ASC
			LIMIT 20`
	}

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
		)

		if err != nil {
			return nil, fmt.Errorf("GetStopRealtimeStopTimes scanning failed: %w", err)
		}

		sts = append(sts, st)
	}

	return sts, nil
}
