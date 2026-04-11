package models

import (
	"time"

	"github.com/twpayne/go-geom"
)

type VehiclePosition struct {
	TripId               string               `json:"trip_id"`
	RouteId              string               `json:"trip_route_id"`
	ScheduleRelationship ScheduleRelationship `json:"trip_schedule_relationship"`
	VehicleId            *string              `json:"vehicle_id"`
	VehicleLabel         *string              `json:"vehicle_label"`
	VehicleModel         string               `json:"vehicle_model"`
	Latitude             float32              `json:"position_latitude"`
	Longitude            float32              `json:"position_longitude"`
	Geom                 *geom.Point          `json:"position_geom"`
	StopId               string               `json:"stop_id"`
	Timestamp            time.Time            `json:"timestamp"`
	CongestionLevel      CongestionLevel      `json:"congestion_level"`
	OccupancyStatus      OccupancyStatus      `json:"occupancy_status"`
	Consist              []Consist            `json:"consist"`
	Mode                 string               `json:"mode"`
}

type Consist struct {
	VehicleId         string          `json:"vehicle_id"`
	PositionInConsist uint            `json:"position_in_consist"`
	OccupancyStatus   OccupancyStatus `json:"occupancy_status"`
}
