package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type VehicleBoardingRepository struct {
	DB *pgxpool.Pool
}

func NewVehicleBoardingRepository(db *pgxpool.Pool) *VehicleBoardingRepository {
	return &VehicleBoardingRepository{DB: db}
}

func (r *VehicleBoardingRepository) GetVehicleBoardings() ([]models.VehicleBoarding, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM vehicle_boardings")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var vehicleBoardings []models.VehicleBoarding
	for rows.Next() {
		var vehicleBoarding models.VehicleBoarding
		err := rows.Scan(
			&vehicleBoarding.VehicleCategoryId,
			&vehicleBoarding.ChildSequence,
			&vehicleBoarding.GrandchildSequence,
			&vehicleBoarding.BoardingAreaId,
		)

		if err != nil {
			return nil, err
		}

		vehicleBoardings = append(vehicleBoardings, vehicleBoarding)
	}

	return vehicleBoardings, nil
}
