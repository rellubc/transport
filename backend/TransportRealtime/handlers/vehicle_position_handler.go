package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

func GetVehiclePositionsHandler(repo *repositories.VehiclePositionRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		mode := r.URL.Query().Get("mode")

		vps, err := repo.GetVehiclePositions(mode)

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
		vpId := chi.URLParam(r, "vehicle_id")

		vp, err := repo.GetVehiclePosition(vpId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(vp)
	}
}
