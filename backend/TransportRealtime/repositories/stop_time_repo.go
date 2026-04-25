package repositories

import (
	models "TransportRealtime/models/static"
	"context"
	"fmt"
	"strings"

	"github.com/jackc/pgx/v5/pgxpool"
)

type StopTimeRepository struct {
	DB *pgxpool.Pool
}

func NewStopTimeRepository(db *pgxpool.Pool) *StopTimeRepository {
	return &StopTimeRepository{DB: db}
}

func (r *StopTimeRepository) GetStaticStopTimes(stopId string, tripId string) ([]models.StaticStopTime, error) {
	baseQuery := "SELECT st.trip_id, st.arrival_time, st.departure_time, st.stop_id, st.stop_sequence, st.pickup_type, st.drop_off_type, st.shape_dist_travelled, st.timepoint, st.stop_note, st.mode, t.trip_headsign AS stop_headsign FROM stop_times st JOIN trips t ON st.trip_id = t.trip_id"
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
			&stopTime.Mode,
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
			AND st.mode = s.mode
		JOIN trips t
			ON st.trip_id = t.trip_id
		LEFT JOIN LATERAL (
			SELECT DISTINCT ON (position_in_consist) position_in_consist, occupancy_status, trip_id
			FROM consist
			WHERE trip_id = st.trip_id 
				AND EXTRACT(EPOCH FROM timestamp AT TIME ZONE 'Australia/Sydney')::integer %% 86400 <= (st.departure_time + COALESCE(stu.stop_departure_delay, 0))
			ORDER BY position_in_consist, timestamp DESC
			LIMIT CASE 
				WHEN st.mode = 'metro' THEN 6
				WHEN st.mode = 'lightrail/innerwest' THEN 2
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
