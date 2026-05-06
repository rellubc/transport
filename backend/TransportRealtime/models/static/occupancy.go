package models

import "time"

type Occupancy struct {
	TripId          string     `json:"tripId"`
	StopSequence    int        `json:"stopSequence"`
	OccupancyStatus int        `json:"occupancyStatus"`
	Monday          bool       `json:"monday"`
	Tuesday         bool       `json:"tuesday"`
	Wednesday       bool       `json:"wednesday"`
	Thursday        bool       `json:"thursday"`
	Friday          bool       `json:"friday"`
	Saturday        bool       `json:"saturday"`
	Sunday          bool       `json:"sunday"`
	StartDate       time.Time  `json:"startDate"`
	EndDate         *time.Time `json:"endDate"`
	Exception       *bool      `json:"exception"`
}
