package models

type VehicleBoarding struct {
	VehicleCategoryId  string `json:"vehicleCategoryId"`
	ChildSequence      int    `json:"childSequence"`
	GrandchildSequence int    `json:"grandchildSequence"`
	BoardingAreaId     string `json:"boardingAreaId"`
}
