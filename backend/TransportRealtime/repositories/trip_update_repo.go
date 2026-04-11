package repositories

import (
	models "TransportRealtime/models/realtime"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type TripUpdateRepository struct {
	DB *pgxpool.Pool
}

func NewTripUpdateRepository(db *pgxpool.Pool) *TripUpdateRepository {
	return &TripUpdateRepository{DB: db}
}

func (r *TripUpdateRepository) GetTripUpdate(tripId string) (models.TripUpdate, error) {
	row := r.DB.QueryRow(context.Background(), "SELECT * FROM tripUpdates WHERE trip_id = $1", tripId)

	var tu models.TripUpdate
	err := row.Scan(
		&tu.TripId,
		&tu.RouteId,
		&tu.ScheduleRelationship,
		&tu.VehicleId,
		&tu.VehicleLabel,
		&tu.VehicleModel,
		&tu.Mode,
	)

	return tu, err
}

func (r *TripUpdateRepository) GetTripStopTimeUpdates(tripId string) ([]models.StopTimeUpdate, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM stop_time_updates WHERE trip_id = $1", tripId)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var stus []models.StopTimeUpdate
	for rows.Next() {
		var stu models.StopTimeUpdate
		err := rows.Scan(
			&stu.TripId,
			&stu.StopId,
			&stu.ArrivalTime,
			&stu.DepartureTime,
		)
		if err != nil {
			return nil, err
		}
		stus = append(stus, stu)
	}

	return stus, nil
}

func (r *TripUpdateRepository) GetTripStopCarriageSequencePredictiveOccupancy(tripId string, stopId string) ([]models.CarriageSequencePredictiveOccupancy, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM carriage_sequence_predictive_occupancy WHERE trip_id = $1 AND stop_id = $2", tripId, stopId)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var cspos []models.CarriageSequencePredictiveOccupancy
	for rows.Next() {
		var cspo models.CarriageSequencePredictiveOccupancy
		err := rows.Scan(
			&cspo.TripId,
			&cspo.StopId,
			&cspo.PositionInConsist,
			&cspo.DepartureOccupancyStatus,
		)
		if err != nil {
			return nil, err
		}
		cspos = append(cspos, cspo)
	}

	return cspos, nil
}
