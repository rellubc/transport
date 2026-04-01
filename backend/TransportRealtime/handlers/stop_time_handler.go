package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetStopTimesHandler(repo *repositories.StopTimeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		stopId := r.URL.Query().Get("stop_id")
		tripId := r.URL.Query().Get("trip_id")

		stopTimes, err := repo.GetStopTimes(stopId, tripId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(stopTimes)
	}
}
