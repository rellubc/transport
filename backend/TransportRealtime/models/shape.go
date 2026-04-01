package models

import "github.com/twpayne/go-geom"

type Shape struct {
	ShapeId            string      `json:"shape_id"`
	ShapePtLat         float64     `json:"shape_pt_lat"`
	ShapePtLon         float64     `json:"shape_pt_lon"`
	ShapeGeom          *geom.Point `json:"shape_geom"`
	ShapePtSequence    int         `json:"shape_pt_sequence"`
	ShapeDistTravelled float64     `json:"shape_dist_traveled"`
	Mode               string      `json:"mode"`
}
