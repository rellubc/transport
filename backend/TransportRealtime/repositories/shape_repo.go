package repositories

import (
	models "TransportRealtime/models/static"
	"context"
	"sort"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
)

type ShapeRepository struct {
	DB *pgxpool.Pool
}

func NewShapeRepository(db *pgxpool.Pool) *ShapeRepository {
	return &ShapeRepository{DB: db}
}

func (r *ShapeRepository) GetShapes(shapeType string) (map[string][]models.ShapeDetails, error) {
	var (
		rows pgx.Rows
		err  error
	)

	query := "SELECT shape_id, shape_pt_lat, shape_pt_lon, shape_pt_sequence FROM shapes"
	prefix := shapeType + "_%"

	rows, err = r.DB.Query(context.Background(), query+" WHERE shape_id LIKE $1", prefix)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	shapeMap := make(map[string][]models.ShapeDetails)

	for rows.Next() {
		var shape models.Shape
		err := rows.Scan(
			&shape.ShapeId,
			&shape.ShapePtLat,
			&shape.ShapePtLon,
			&shape.ShapePtSequence,
		)

		if err != nil {
			return nil, err
		}

		details := models.ShapeDetails{
			ShapePtLat:      shape.ShapePtLat,
			ShapePtLon:      shape.ShapePtLon,
			ShapePtSequence: shape.ShapePtSequence,
		}

		shapeMap[shape.ShapeId] = append(shapeMap[shape.ShapeId], details)
	}

	for shapeId := range shapeMap {
		sort.Slice(shapeMap[shapeId], func(i, j int) bool {
			return shapeMap[shapeId][i].ShapePtSequence < shapeMap[shapeId][j].ShapePtSequence
		})
	}

	return shapeMap, nil
}
