package repositories

import (
	"TransportRealtime/models"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type TripRepository struct {
	DB *pgxpool.Pool
}

func NewTripRepository(db *pgxpool.Pool) *TripRepository {
	return &TripRepository{DB: db}
}

func (r *TripRepository) GetTrips() ([]models.Trip, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM trips")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var trips []models.Trip
	for rows.Next() {
		var trip models.Trip
		err := rows.Scan(
			&trip.RouteId,
			&trip.ServiceId,
			&trip.TripId,
			&trip.TripHeadsign,
			&trip.TripShortName,
			&trip.TripDirectionId,
			&trip.TripBlockId,
			&trip.ShapeId,
			&trip.TripWheelchairAccessible,
			&trip.TripRouteDirection,
			&trip.TripBikesAllowed,
			&trip.TripNote,
			&trip.VehicleCategoryId,
		)

		if err != nil {
			return nil, err
		}

		trips = append(trips, trip)
	}

	return trips, nil
}

func (r *TripRepository) GetTrip(tripId string) (models.Trip, error) {
	row := r.DB.QueryRow(context.Background(), "SELECT * FROM trips WHERE trip_id = $1", tripId)

	var trip models.Trip
	err := row.Scan(
		&trip.RouteId,
		&trip.ServiceId,
		&trip.TripId,
		&trip.TripHeadsign,
		&trip.TripShortName,
		&trip.TripDirectionId,
		&trip.TripBlockId,
		&trip.ShapeId,
		&trip.TripWheelchairAccessible,
		&trip.TripRouteDirection,
		&trip.TripBikesAllowed,
		&trip.TripNote,
		&trip.VehicleCategoryId,
	)

	return trip, err
}
