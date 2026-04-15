package models

import "github.com/twpayne/go-geom"

type Shape struct {
	ShapeId            string      `json:"shapeId"`
	ShapePtLat         float64     `json:"shapePtLat"`
	ShapePtLon         float64     `json:"shapePtLon"`
	ShapeGeom          *geom.Point `json:"shapeGeom"`
	ShapePtSequence    int         `json:"shapePtSequence"`
	ShapeDistTravelled *float64    `json:"shapeDistTravelled"`
	Mode               string      `json:"mode"`
}

type ShapeDetails struct {
	ShapePtLat         float64     `json:"shapePtLat"`
	ShapePtLon         float64     `json:"shapePtLon"`
	ShapeGeom          *geom.Point `json:"shapeGeom"`
	ShapePtSequence    int         `json:"shapePtSequence"`
	ShapeDistTravelled *float64    `json:"shapeDistTravelled"`
	Mode               string      `json:"mode"`
}
