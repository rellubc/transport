package models

type Trip struct {
	RouteId                  string  `json:"routeId"`
	ServiceId                string  `json:"serviceId"`
	TripId                   string  `json:"tripId"`
	TripHeadsign             *string `json:"tripHeadsign"`
	TripShortName            *string `json:"tripShortName"`
	TripDirectionId          int     `json:"tripDirectionId"`
	TripBlockId              *string `json:"tripBlockId"`
	ShapeId                  string  `json:"shapeId"`
	TripWheelchairAccessible int     `json:"tripWheelchairAccessible"`
	TripRouteDirection       *string `json:"tripRouteDirection"`
	TripBikesAllowed         *int    `json:"tripBikesAllowed"`
	TripNote                 *string `json:"tripNote"`
	VehicleCategoryId        *string `json:"vehicleCategoryId"`
}
