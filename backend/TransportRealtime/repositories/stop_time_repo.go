package repositories

import (
	models "TransportRealtime/models/static"
	"context"
	"encoding/json"
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
						'positionInConsist', co.position_in_consist,
						'departureOccupancyStatus', co.departure_occupancy_status
					)
				) FILTER (WHERE co.trip_id IS NOT NULL),
				'[]'
			) AS carriage_occupancies
		FROM stop_times st
		LEFT JOIN stop_time_updates_with_progress stu
			ON st.stop_id = stu.stop_id
			AND st.trip_id = stu.trip_id
		JOIN stops s
			ON st.stop_id = s.stop_id
		LEFT JOIN carriage_occupancies co
			ON st.stop_id = co.stop_id
			AND st.trip_id = co.trip_id
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

	log.Println(baseQuery, args)

	rows, err := r.DB.Query(context.Background(), baseQuery, args...)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var sts []models.RealtimeStopTime
	for rows.Next() {
		var st models.RealtimeStopTime
		var occupanciesJSON []byte

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
			&occupanciesJSON,
		)

		if err != nil {
			return nil, err
		}

		err = json.Unmarshal(occupanciesJSON, &st.CarriageOccupancies)
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
