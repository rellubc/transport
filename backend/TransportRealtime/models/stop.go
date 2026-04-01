package models

type Stop struct {
	StopId                 string  `json:"stop_id"`
	StopCode               string  `json:"stop_code"`
	StopName               string  `json:"stop_name"`
	StopLat                float64 `json:"stop_lat"`
	StopLon                float64 `json:"stop_lon"`
	StopZoneId             string  `json:"stop_zone_id"`
	StopUrl                string  `json:"stop_url"`
	StopLocationType       int     `json:"stop_location_type"`
	StopParentStationId    string  `json:"stop_parent_station"`
	StopTimezone           string  `json:"stop_timezone"`
	StopWheelchairBoarding bool    `json:"stop_wheelchair_boarding"`
	StopPlatformCode       int     `json:"stop_platform_code"`
	Mode                   string  `json:"mode"`
}
