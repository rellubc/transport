package models

import "time"

type StopTime struct {
	TripId            string    `json:"trip_id"`
	ArrivalTime       time.Time `json:"arrival_time"`
	DepartureTime     time.Time `json:"departure_time"`
	StopId            string    `json:"stop_id"`
	StopSequence      int       `json:"stop_sequence"`
	StopHeadsign      string    `json:"stop_headsign"`
	PickupType        int       `json:"pickup_type"`
	DropOffType       int       `json:"drop_off_type"`
	ShapeDistTraveled float64   `json:"shape_dist_traveled"`
	Timepoint         int       `json:"timepoint"`
	StopNote          string    `json:"stop_note"`
	Mode              string    `json:"mode"`
}
