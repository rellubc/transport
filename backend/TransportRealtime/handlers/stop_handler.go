package handlers

import (
	models "TransportRealtime/models/static"
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
	"strings"

	"github.com/go-chi/chi/v5"
)

func GetStopsHandler(repo *repositories.StopRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		mode := strings.ToLower(r.URL.Query().Get("mode"))

		var stops map[string][]models.Stop
		var err error

		if mode != "" {
			stops, err = repo.GetStops(mode)
		} else {
			stops, err = repo.GetStops("")
		}

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
