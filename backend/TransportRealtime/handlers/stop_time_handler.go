package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
	"strconv"

	"github.com/go-chi/chi/v5"
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

func GetStopStopTimesHandler(repo *repositories.StopTimeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		stopId := chi.URLParam(r, "stop_id")
		direction := r.URL.Query().Get("direction")
		timeStr := r.URL.Query().Get("time")

		time, _ := strconv.Atoi(timeStr)

		stop, err := repo.GetStopStopTimes(stopId, direction, time)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(stop)
	}
}

func GetTripStopTimesHandler(repo *repositories.StopTimeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		tripId := chi.URLParam(r, "trip_id")
		vehicleLon := r.URL.Query().Get("vehicle_lon")
		vehicleLat := r.URL.Query().Get("vehicle_lat")

		vehicleLonF, err := strconv.ParseFloat(vehicleLon, 32)
		if err != nil {
			http.Error(w, "invalid vehicle_lon", http.StatusBadRequest)
			return
		}
		vehicleLatF, err := strconv.ParseFloat(vehicleLat, 32)
		if err != nil {
			http.Error(w, "invalid vehicle_lat", http.StatusBadRequest)
			return
		}

		stop, err := repo.GetTripStopTimes(tripId, vehicleLonF, vehicleLatF)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(stop)
	}
}
