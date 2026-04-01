package models

type Route struct {
	RouteId          string `json:"route_id"`
	AgencyId         string `json:"agency_id"`
	RouteShortName   string `json:"route_short_name"`
	RouteLongName    string `json:"route_long_name"`
	RouteDescription string `json:"route_desc"`
	RouteType        int    `json:"route_type"`
	RouteColor       string `json:"route_color"`
	RouteTextColor   string `json:"route_text_color"`
	RouteUrl         string `json:"route_url"`
}
