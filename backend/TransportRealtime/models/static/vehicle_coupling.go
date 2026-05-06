package models

type VehicleCoupling struct {
	ParentId      string `json:"parentId"`
	ChildId       string `json:"childId"`
	ChildSequence int    `json:"childSequence"`
	ChildLabel    string `json:"childLabel"`
}
