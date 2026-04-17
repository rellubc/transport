package repositories

import (
	models "TransportRealtime/models/static"
	"context"
	"encoding/json"
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
	baseQuery := "SELECT * FROM stop_times"
	args := []any{}
	conditions := []string{}

	if stopId != "" {
		conditions = append(conditions, fmt.Sprintf("stop_id = $%d", len(args)+1))
		args = append(args, stopId)
	}

	if tripId != "" {
		conditions = append(conditions, fmt.Sprintf("trip_id = $%d", len(args)+1))
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
			&stopTime.StopHeadsign,
			&stopTime.PickupType,
			&stopTime.DropOffType,
			&stopTime.ShapeDistTravelled,
			&stopTime.Timepoint,
			&stopTime.StopNote,
			&stopTime.Mode,
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
			st.stop_headsign,
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
		LEFT JOIN LATERAL (
			SELECT position_in_consist, occupancy_status, trip_id
			FROM consist
			WHERE trip_id = st.trip_id 
				AND EXTRACT(EPOCH FROM timestamp AT TIME ZONE 'Australia/Sydney')::integer % 86400 <= (st.departure_time + COALESCE(stu.stop_departure_delay, 0))
			ORDER BY timestamp DESC
			LIMIT CASE WHEN st.mode = 'metro' THEN 6 ELSE 8 END
		) c ON true
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

	if len(conditions) > 0 {
		baseQuery += "WHERE " + strings.Join(conditions, " AND ")
	}

	baseQuery += `
		GROUP BY
			st.stop_id,
			s.stop_name,
			st.stop_sequence,
			st.stop_headsign,
			st.arrival_time,
			stu.stop_arrival_delay,
			st.departure_time,
			stu.stop_departure_delay,
			stu.progress
	`
	baseQuery += "ORDER BY st.stop_sequence"

	rows, err := r.DB.Query(context.Background(), baseQuery, args...)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var sts []models.RealtimeStopTime
	for rows.Next() {
		var st models.RealtimeStopTime
		var consistJSON []byte

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
			&consistJSON,
		)

		if err != nil {
			return nil, err
		}

		err = json.Unmarshal(consistJSON, &st.Consist)
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
