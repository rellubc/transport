package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetAgenciesHandler(repo *repositories.AgencyRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		agencies, err := repo.GetAgencies()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(agencies)
	}
}

func GetAgencyHandler(repo *repositories.AgencyRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		id := r.URL.Query().Get("id")

		agency, err := repo.GetAgency(id)
		if err != nil {
			http.Error(w, "Failed to fetch agency "+id, http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(agency)
	}
}
