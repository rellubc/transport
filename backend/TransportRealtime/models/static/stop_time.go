package models

import models "TransportRealtime/models/realtime"

type StaticStopTime struct {
	TripId             string   `json:"tripId"`
	StopId             string   `json:"stopId"`
	StopName           string   `json:"stopName"`
	StopSequence       int      `json:"stopSequence"`
	StopHeadsign       *string  `json:"stopHeadsign"`
	ArrivalTime        int      `json:"arrivalTime"`
	DepartureTime      int      `json:"departureTime"`
	PickupType         int      `json:"pickupType"`
	DropOffType        int      `json:"dropOffType"`
	ShapeDistTravelled *float64 `json:"shapeDistTravelled"`
	Timepoint          *int     `json:"timepoint"`
	StopNote           *string  `json:"stopNote"`
	RouteType          *int     `json:"routeType"`
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
	Progress       *string          `json:"progress"`
	Consist        []models.Consist `json:"consist"`
}

type StopRealtimeStopTime struct {
	TripId                 string  `json:"tripId"`
	TripHeadsign           *string `json:"tripHeadsign"`
	ServiceId              string  `json:"serviceId"`
	RouteId                string  `json:"routeId"`
	RouteShortName         *string `json:"routeShortName"`
	RouteType              *int    `json:"routeType"`
	RouteColour            *string `json:"routeColour"`
	StopId                 string  `json:"stopId"`
	StopName               string  `json:"stopName"`
	ArrivalTime            *int    `json:"arrivalTime"`
	ArrivalDelay           *int    `json:"arrivalDelay"`
	DepartureTime          *int    `json:"departureTime"`
	DepartureDelay         *int    `json:"departureDelay"`
	EffectiveArrivalTime   *int    `json:"effectiveArrivalTime"`
	EffectiveDepartureTime *int    `json:"effectiveDepartureTime"`
	PickupType             int     `json:"pickupType"`
	DropOffType            int     `json:"dropOffType"`
	StopType               string  `json:"stopType"`
	IsRealtime             bool    `json:"isRealtime"`
	DisplayTime            int     `json:"displayTime"`
	HasContinuation        bool    `json:"hasContinuation"`
}

type TripRealtimeStopTime struct {
	TripHeadsign           *string          `json:"tripHeadsign"`
	RouteShortName         *string          `json:"routeShortName"`
	RouteType              *int             `json:"routeType"`
	RouteColour            *string          `json:"routeColour"`
	StopId                 string           `json:"stopId"`
	StopName               string           `json:"stopName"`
	ArrivalDelay           *int             `json:"arrivalDelay"`
	DepartureDelay         *int             `json:"departureDelay"`
	EffectiveArrivalTime   *int             `json:"effectiveArrivalTime"`
	EffectiveDepartureTime *int             `json:"effectiveDepartureTime"`
	StopType               string           `json:"stopType"`
	Progress               *string          `json:"progress"`
	IsRealtime             bool             `json:"isRealtime"`
	Consist                []models.Consist `json:"consist"`
	DisplayTime            int              `json:"displayTime"`
	StopProgress           float64          `json:"stopProgress"`
}
