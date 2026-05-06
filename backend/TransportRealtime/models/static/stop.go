package models

import "github.com/twpayne/go-geom"

type Stop struct {
	StopId                 string      `json:"stopId"`
	StopCode               *string     `json:"stopCode"`
	StopName               string      `json:"stopName"`
	StopDescription        *string     `json:"stopDescription"`
	StopLat                float64     `json:"stopLat"`
	StopLon                float64     `json:"stopLon"`
	StopGeom               *geom.Point `json:"stopGeom"`
	StopZoneId             *string     `json:"stopZoneId"`
	StopUrl                *string     `json:"stopUrl"`
	StopLocationType       *int        `json:"stopLocationType"`
	StopParentStation      *string     `json:"stopParentStation"`
	StopTimezone           *string     `json:"stopTimezone"`
	StopWheelchairBoarding int         `json:"stopWheelchairBoarding"`
	StopPlatformCode       *string     `json:"stopPlatformCode"`
	RouteType              *int        `json:"routeType"`
}
