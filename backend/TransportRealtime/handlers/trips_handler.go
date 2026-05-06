package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

func GetTripsHandler(repo *repositories.TripRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		trips, err := repo.GetTrips()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(trips)
	}
}

func GetTripHandler(repo *repositories.TripRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		tripId := chi.URLParam(r, "trip_id")

		trip, err := repo.GetTrip(tripId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(trip)
	}
}
