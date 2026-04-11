package models

type VehicleCoupling struct {
	ParentId      string `json:"parent_id"`
	ChildId       string `json:"child_id"`
	ChildSequence int    `json:"child_sequence"`
	ChildLabel    string `json:"child_label"`
}
