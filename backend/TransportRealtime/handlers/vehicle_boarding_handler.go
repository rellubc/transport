package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetVehicleBoardingsHandler(repo *repositories.VehicleBoardingRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		vehicleBoardings, err := repo.GetVehicleBoardings()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(vehicleBoardings)
	}
}
