package models

import "time"

type TripUpdate struct {
	TripId               string               `json:"trip_id"`
	RouteId              string               `json:"trip_route_id"`
	ScheduleRelationship ScheduleRelationship `json:"trip_schedule_relationship"`
	VehicleId            *string              `json:"vehicle_id"`
	VehicleLabel         *string              `json:"vehicle_label"`
	VehicleModel         string               `json:"vehicle_model"`
	StopTimeUpdates      []StopTimeUpdate
	Timestamp            time.Time `json:"timestamp"`
	Mode                 string    `json:"mode"`
}

type StopTimeUpdate struct {
	TripId        string    `json:"trip_id"`
	StopId        string    `json:"stop_id"`
	ArrivalTime   time.Time `json:"stop_arrival_time"`
	DepartureTime time.Time `json:"stop_departure_time"`
}

type CarriageSequencePredictiveOccupancy struct {
	TripId                   string          `json:"trip_id"`
	StopId                   string          `json:"stop_id"`
	PositionInConsist        uint            `json:"position_in_consist"`
	DepartureOccupancyStatus OccupancyStatus `json:"departure_occupancy_status"`
}
