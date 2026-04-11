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

func (r *ShapeRepository) GetShapes(lines []string) (map[string][]models.ShapeDetails, error) {
	var (
		rows pgx.Rows
		err  error
	)

	query := "SELECT shape_id, shape_pt_lat, shape_pt_lon FROM shapes"

	if len(lines) == 0 {
		rows, err = r.DB.Query(context.Background(), query+" WHERE shape_id ~ '^[A-Z][0-9]+_[0-9]$'")
	} else {
		patterns := []string{}
		for _, l := range lines {
			patterns = append(patterns, string(l)+"_%")
		}
		rows, err = r.DB.Query(context.Background(), query+" WHERE shape_id LIKE ANY($1) AND shape_id ~ '^[A-Z][0-9]+_[0-9]$'", patterns)
	}

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
		)

		if err != nil {
			return nil, err
		}

		details := models.ShapeDetails{
			ShapePtLat: shape.ShapePtLat,
			ShapePtLon: shape.ShapePtLon,
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

// package repositories

// import (
// 	models "TransportRealtime/models/static"

// 	"context"
// 	"sort"
// 	"strings"

// 	"github.com/jackc/pgx/v5"
// 	"github.com/jackc/pgx/v5/pgxpool"
// )

// type ShapeRepository struct {
// 	DB *pgxpool.Pool
// }

// func NewShapeRepository(db *pgxpool.Pool) *ShapeRepository {
// 	return &ShapeRepository{DB: db}
// }

// func (r *ShapeRepository) GetShapes(mode string) (map[string][]models.Shape, error) {
// 	var (
// 		rows pgx.Rows
// 		err  error
// 	)

// 	query := "SELECT shape_id, shape_pt_lat, shape_pt_lon, shape_pt_sequence, shape_dist_travelled, mode FROM shapes"

// 	if mode == "" {
// 		rows, err = r.DB.Query(context.Background(), query)
// 	} else {
// 		rows, err = r.DB.Query(context.Background(), query+" WHERE mode = $1", mode)
// 	}

// 	if err != nil {
// 		return nil, err
// 	}
// 	defer rows.Close()

// 	shapeMap := make(map[string][]models.Shape)

// 	for rows.Next() {
// 		var shape models.Shape
// 		err := rows.Scan(
// 			&shape.ShapeId,
// 			&shape.ShapePtLat,
// 			&shape.ShapePtLon,
// 			&shape.ShapePtSequence,
// 			&shape.ShapeDistTravelled,
// 			&shape.Mode,
// 		)

// 		if err != nil {
// 			return nil, err
// 		}

// 		shapeMap[strings.Split(shape.ShapeId, "_")[0]] = append(shapeMap[strings.Split(shape.ShapeId, "_")[0]], shape)
// 	}

// 	for shapeId := range shapeMap {
// 		sort.Slice(shapeMap[shapeId], func(i, j int) bool {
// 			return shapeMap[shapeId][i].ShapePtSequence < shapeMap[shapeId][j].ShapePtSequence
// 		})
// 	}

// 	return shapeMap, nil
// }
