package models

type Agency struct {
	AgencyId       string  `json:"agency_id"`
	AgencyName     string  `json:"agency_name"`
	AgencyUrl      string  `json:"agency_url"`
	AgencyTimezone string  `json:"agency_timezone"`
	AgencyLanguage *string `json:"agency_lang"`
	AgencyPhone    *string `json:"agency_phone"`
	AgencyFareUrl  *string `json:"agency_fare_url"`
	AgencyEmail    *string `json:"agency_email"`
}
