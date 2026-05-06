package models

type Route struct {
	RouteId          string  `json:"routeId"`
	AgencyId         string  `json:"agencyUd"`
	RouteShortName   *string `json:"routeShortName"`
	RouteLongName    string  `json:"routeLongName"`
	RouteDescription string  `json:"routeDesc"`
	RouteType        int     `json:"routeType"`
	RouteColor       string  `json:"routeColor"`
	RouteTextColor   string  `json:"routeTextColor"`
	RouteUrl         *string `json:"routeUrl"`
}
