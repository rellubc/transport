package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type OccupancyRepository struct {
	DB *pgxpool.Pool
}

func NewOccupancyRepository(db *pgxpool.Pool) *OccupancyRepository {
	return &OccupancyRepository{DB: db}
}

func (r *OccupancyRepository) GetOccupancies() ([]models.Occupancy, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM occupancies")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var occupancies []models.Occupancy
	for rows.Next() {
		var occupancy models.Occupancy
		err := rows.Scan(
			&occupancy.TripId,
			&occupancy.StopSequence,
			&occupancy.OccupancyStatus,
			&occupancy.Monday,
			&occupancy.Tuesday,
			&occupancy.Wednesday,
			&occupancy.Thursday,
			&occupancy.Friday,
			&occupancy.Saturday,
			&occupancy.Sunday,
			&occupancy.StartDate,
			&occupancy.EndDate,
			&occupancy.Exception,
		)

		if err != nil {
			return nil, err
		}

		occupancies = append(occupancies, occupancy)
	}

	return occupancies, nil
}
