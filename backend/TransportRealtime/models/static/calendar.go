package models

import "time"

type Calendar struct {
	ServiceId string    `json:"serviceI"`
	Monday    bool      `json:"monday"`
	Tuesday   bool      `json:"tuesday"`
	Wednesday bool      `json:"wednesday"`
	Thursday  bool      `json:"thursday"`
	Friday    bool      `json:"friday"`
	Saturday  bool      `json:"saturday"`
	Sunday    bool      `json:"sunday"`
	StartDate time.Time `json:"startDate"`
	EndDate   time.Time `json:"endDate"`
}
