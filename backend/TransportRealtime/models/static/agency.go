package models

type Agency struct {
	AgencyId       string  `json:"agencyId"`
	AgencyName     string  `json:"agencyName"`
	AgencyUrl      string  `json:"agencyUrl"`
	AgencyTimezone string  `json:"agencyTimezone"`
	AgencyLanguage *string `json:"agencyLang"`
	AgencyPhone    *string `json:"agencyPone"`
	AgencyFareUrl  *string `json:"agencyFareUrl"`
	AgencyEmail    *string `json:"agencyEmail"`
}
