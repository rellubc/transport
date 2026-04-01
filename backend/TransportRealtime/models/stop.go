package models

import "github.com/twpayne/go-geom"

type Stop struct {
	StopId                 string      `json:"stop_id"`
	StopCode               *string     `json:"stop_code"`
	StopName               string      `json:"stop_name"`
	StopLat                float64     `json:"stop_lat"`
	StopLon                float64     `json:"stop_lon"`
	StopGeom               *geom.Point `json:"stop_geom"`
	StopZoneId             *string     `json:"stop_zone_id"`
	StopUrl                *string     `json:"stop_url"`
	StopLocationType       int         `json:"stop_location_type"`
	StopParentStation      *string     `json:"stop_parent_station"`
	StopTimezone           *string     `json:"stop_timezone"`
	StopWheelchairBoarding int         `json:"stop_wheelchair_boarding"`
	StopPlatformCode       *string     `json:"stop_platform_code"`
	Mode                   string      `json:"mode"`
}
