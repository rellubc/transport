package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
	"strings"

	"github.com/go-chi/chi/v5"
)

func GetTripUpdateHandler(repo *repositories.TripUpdateRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		tripId := strings.ToLower(r.URL.Query().Get("trip_id"))
		tus, err := repo.GetTripUpdate(tripId)

		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(tus)
	}
}

func GetTripStopTimeUpdatesHandler(repo *repositories.TripUpdateRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		tripId := chi.URLParam(r, "trip_id")

		cspos, err := repo.GetTripStopTimeUpdates(tripId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(cspos)
	}
}
