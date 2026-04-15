package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetStaticStopTimesHandler(repo *repositories.StopTimeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		stopId := r.URL.Query().Get("stop_id")
		tripId := r.URL.Query().Get("trip_id")

		stopTimes, err := repo.GetStaticStopTimes(stopId, tripId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(stopTimes)
	}
}

func GetRealtimeStopTimesHandler(repo *repositories.StopTimeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		stopId := r.URL.Query().Get("stop_id")
		tripId := r.URL.Query().Get("trip_id")

		stopTimes, err := repo.GetRealtimeStopTimes(stopId, tripId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(stopTimes)
	}
}
