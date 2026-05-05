package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

func GetStopsHandler(repo *repositories.StopRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {

		stops, err := repo.GetStops()

		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(stops)
	}
}

func GetStopHandler(repo *repositories.StopRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		stopId := chi.URLParam(r, "stop_id")

		stop, err := repo.GetStop(stopId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(stop)
	}
}
