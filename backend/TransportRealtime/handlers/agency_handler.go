package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
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
		agencyId := chi.URLParam(r, "agency_id")

		agency, err := repo.GetAgency(agencyId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(agency)
	}
}
