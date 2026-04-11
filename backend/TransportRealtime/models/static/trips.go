package models

type Trip struct {
	RouteId                  string `json:"route_id"`
	ServiceId                string `json:"service_id"`
	TripId                   string `json:"trip_id"`
	TripHeadsign             string `json:"trip_headsign"`
	TripShortName            string `json:"trip_short_name"`
	TripDirectionId          string `json:"trip_direction_id"`
	TripBlockId              string `json:"trip_block_id"`
	ShapeId                  string `json:"shape_id"`
	TripWheelchairAccessible int    `json:"trip_wheelchair_accessible"`
	TripRouteDirection       string `json:"trip_route_direction"`
	TripBikesAllowed         int    `json:"trip_bikes_allowed"`
	TripNote                 string `json:"trip_note"`
	VehicleCategoryId        string `json:"vehicle_category_id"`
}
