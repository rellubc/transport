package models

import models "TransportRealtime/models/realtime"

type StaticStopTime struct {
	TripId             string   `json:"tripId"`
	StopId             string   `json:"stopId"`
	StopName           string   `json:"stopName"`
	ArrivalTime        int      `json:"arrivalTime"`
	DepartureTime      int      `json:"departureTime"`
	StopSequence       int      `json:"stopSequence"`
	StopHeadsign       *string  `json:"stopHeadsign"`
	PickupType         int      `json:"pickupType"`
	DropOffType        int      `json:"dropOffType"`
	ShapeDistTravelled *float64 `json:"shapeDistTravelled"`
	Timepoint          *int     `json:"timepoint"`
	StopNote           *string  `json:"stopNote"`
	Mode               string   `json:"mode"`
}

type RealtimeStopTime struct {
	StopId         string           `json:"stopId"`
	StopName       string           `json:"stopName"`
	StopSequence   int              `json:"stopSequence"`
	StopHeadsign   *string          `json:"stopHeadsign"`
	ArrivalTime    *int             `json:"arrivalTime"`
	DepartureTime  *int             `json:"departureTime"`
	ArrivalDelay   *int             `json:"arrivalDelay"`
	DepartureDelay *int             `json:"departureDelay"`
	Status         string           `json:"status"`
	Progress       string           `json:"progress"`
	Consist        []models.Consist `json:"consist"`
}
