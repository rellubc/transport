package models

import "time"

type Occupancy struct {
	TripId          string     `json:"trip_id"`
	StopSequence    int        `json:"stop_sequence"`
	OccupancyStatus int        `json:"occupancy_status"`
	Monday          bool       `json:"monday"`
	Tuesday         bool       `json:"tuesday"`
	Wednesday       bool       `json:"wednesday"`
	Thursday        bool       `json:"thursday"`
	Friday          bool       `json:"friday"`
	Saturday        bool       `json:"saturday"`
	Sunday          bool       `json:"sunday"`
	StartDate       time.Time  `json:"start_date"`
	EndDate         *time.Time `json:"end_date"`
	Exception       *bool      `json:"exception"`
}
