package repositories

import (
	"TransportRealtime/models"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type VehicleCategoryRepository struct {
	DB *pgxpool.Pool
}

func NewVehicleCategoryRepository(db *pgxpool.Pool) *VehicleCategoryRepository {
	return &VehicleCategoryRepository{DB: db}
}

func (r *VehicleCategoryRepository) GetVehicleCategories() ([]models.VehicleCategory, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM vehicle_categories")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var vehicleCategories []models.VehicleCategory
	for rows.Next() {
		var vehicleCategory models.VehicleCategory
		err := rows.Scan(
			&vehicleCategory.VehicleCategoryId,
			&vehicleCategory.VehicleCategoryName,
		)

		if err != nil {
			return nil, err
		}

		vehicleCategories = append(vehicleCategories, vehicleCategory)
	}

	return vehicleCategories, nil
}
