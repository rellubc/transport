package handlers

import (
	models "TransportRealtime/models/realtime"
	"TransportRealtime/repositories"
	"encoding/json"
	"log"
	"net/http"
	"strconv"
)

func GetVehiclePositionsHandler(repo *repositories.VehiclePositionRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		routeTypeStr := r.URL.Query().Get("route_type")

		var vps map[int][]models.VehiclePosition
		var err error

		if routeTypeStr == "" {
			vps, err = repo.GetVehiclePositions(nil)
		} else {
			routeType, err := strconv.Atoi(routeTypeStr)
			if err != nil {
				http.Error(w, "route_type must be an integer", http.StatusBadRequest)
				return
			}
			vps, err = repo.GetVehiclePositions(&routeType)
		}

		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(vps)
	}
}

func GetVehiclePositionHandler(repo *repositories.VehiclePositionRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		vpId := r.URL.Query().Get("vehicle_id")
		tripId := r.URL.Query().Get("trip_id")

		log.Println(vpId, tripId)

		var vp models.VehiclePosition
		var err error
		if vpId == "" && tripId != "" {
			vp, err = repo.GetVehiclePosition("", tripId)
		} else if vpId != "" && tripId == "" {
			vp, err = repo.GetVehiclePosition(vpId, "")
		}
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(vp)
	}
}
