package models

type VehicleBoarding struct {
	VehicleCategoryId  string `json:"vehicle_category_id"`
	ChildSequence      int    `json:"child_sequence"`
	GrandchildSequence int    `json:"grandchild_sequence"`
	BoardingAreaId     string `json:"boarding_area_id"`
}
