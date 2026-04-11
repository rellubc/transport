package repositories

import (
	models "TransportRealtime/models/static"
	"context"
	"strings"

	"github.com/jackc/pgx/v5/pgxpool"
)

type StopTimeRepository struct {
	DB *pgxpool.Pool
}

func NewStopTimeRepository(db *pgxpool.Pool) *StopTimeRepository {
	return &StopTimeRepository{DB: db}
}

func (r *StopTimeRepository) GetStopTimes(stopId string, tripId string) ([]models.StopTime, error) {
	baseQuery := "SELECT * FROM stop_times"
	args := []any{}
	conditions := []string{}

	if stopId != "" {
		conditions = append(conditions, "stop_id = $1")
		args = append(args, stopId)
	}

	if tripId != "" {
		conditions = append(conditions, "trip_id = $2")
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

	var stopTimes []models.StopTime
	for rows.Next() {
		var stopTime models.StopTime
		err := rows.Scan(
			&stopTime.TripId,
			&stopTime.ArrivalTime,
			&stopTime.DepartureTime,
			&stopTime.StopId,
			&stopTime.StopSequence,
			&stopTime.StopHeadsign,
			&stopTime.PickupType,
			&stopTime.DropOffType,
			&stopTime.ShapeDistTraveled,
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
