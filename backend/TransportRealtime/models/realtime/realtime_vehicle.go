package models

import (
	"time"

	"github.com/twpayne/go-geom"
)

type VehiclePosition struct {
	TripId               string      `json:"tripId"`
	RouteId              string      `json:"tripRouteId"`
	ScheduleRelationship string      `json:"tripScheduleRelationship"`
	VehicleId            *string     `json:"vehicleId"`
	VehicleLabel         *string     `json:"vehicleLabel"`
	VehicleModel         string      `json:"vehicleModel"`
	Latitude             float32     `json:"positionLatitude"`
	Longitude            float32     `json:"positionLongitude"`
	Geom                 *geom.Point `json:"positionGeom"`
	StopId               string      `json:"stopId"`
	Timestamp            time.Time   `json:"timestamp"`
	CongestionLevel      string      `json:"congestionLevel"`
	OccupancyStatus      *string     `json:"occupancyStatus"`
	Mode                 string      `json:"mode"`
}

type Consist struct {
	PositionInConsist uint   `json:"positionInConsist"`
	OccupancyStatus   string `json:"occupancyStatus"`
}
