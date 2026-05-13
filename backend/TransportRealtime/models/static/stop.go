package models

import "github.com/twpayne/go-geom"

type Stop struct {
	StopId                 string      `json:"stopId"`
	StopName               string      `json:"stopName"`
	StopLat                float64     `json:"stopLat"`
	StopLon                float64     `json:"stopLon"`
	StopGeom               *geom.Point `json:"stopGeom"`
	StopParentStation      *string     `json:"stopParentStation"`
	StopWheelchairBoarding int         `json:"stopWheelchairBoarding"`
	RouteType              *int        `json:"routeType"`
}
