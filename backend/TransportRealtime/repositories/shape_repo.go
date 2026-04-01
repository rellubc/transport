package repositories

import (
	"TransportRealtime/models"
	"context"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
)

type ShapeRepository struct {
	DB *pgxpool.Pool
}

func NewShapeRepository(db *pgxpool.Pool) *ShapeRepository {
	return &ShapeRepository{DB: db}
}

func (r *ShapeRepository) GetShapes(mode string) ([]models.Shape, error) {
	var (
		rows pgx.Rows
		err  error
	)

	query := "SELECT shape_id, shape_pt_lat, shape_pt_lon, shape_pt_sequence, shape_dist_travelled, mode FROM shapes"

	if mode == "" {
		rows, err = r.DB.Query(context.Background(), query)
	} else {
		rows, err = r.DB.Query(context.Background(), query+"WHERE mode = $1", mode)
	}

	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var shapes []models.Shape
	for rows.Next() {
		var shape models.Shape
		err := rows.Scan(
			&shape.ShapeId,
			&shape.ShapePtLat,
			&shape.ShapePtLon,
			&shape.ShapePtSequence,
			&shape.ShapeDistTravelled,
			&shape.Mode,
		)

		if err != nil {
			return nil, err
		}

		shapes = append(shapes, shape)
	}

	return shapes, nil
}
