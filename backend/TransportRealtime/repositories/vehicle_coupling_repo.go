package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type VehicleCouplingRepository struct {
	DB *pgxpool.Pool
}

func NewVehicleCouplingRepository(db *pgxpool.Pool) *VehicleCouplingRepository {
	return &VehicleCouplingRepository{DB: db}
}

func (r *VehicleCouplingRepository) GetVehicleCouplings() ([]models.VehicleCoupling, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM vehicle_couplings")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var vehicleCouplings []models.VehicleCoupling
	for rows.Next() {
		var vehicleCoupling models.VehicleCoupling
		err := rows.Scan(
			&vehicleCoupling.ParentId,
			&vehicleCoupling.ChildId,
			&vehicleCoupling.ChildSequence,
			&vehicleCoupling.ChildLabel,
		)

		if err != nil {
			return nil, err
		}

		vehicleCouplings = append(vehicleCouplings, vehicleCoupling)
	}

	return vehicleCouplings, nil
}
