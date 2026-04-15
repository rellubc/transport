package models

import "time"

type TripUpdate struct {
	TripId               string  `json:"tripId"`
	RouteId              string  `json:"tripRouteId"`
	ScheduleRelationship string  `json:"tripScheduleRelationship"`
	VehicleId            *string `json:"vehicleId"`
	VehicleLabel         *string `json:"vehicleLabel"`
	VehicleModel         string  `json:"vehicleModel"`
	StopTimeUpdates      []StopTimeUpdate
	Timestamp            time.Time `json:"timestamp"`
	Mode                 string    `json:"mode"`
}

type StopTimeUpdate struct {
	TripId         string    `json:"tripId"`
	StopId         string    `json:"stopId"`
	ArrivalTime    time.Time `json:"stopArrivalTime"`
	DepartureTime  time.Time `json:"stopDepartureTime"`
	ArrivalDelay   int64     `json:"stopArrivalDelay"`
	DepartureDelay int64     `json:"stopDepartureDelay"`
}

type CarriageOccupancy struct {
	PositionInConsist        uint   `json:"positionInConsist"`
	DepartureOccupancyStatus string `json:"departureOccupancyStatus"`
}
