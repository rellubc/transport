package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetVehicleCouplingsHandler(repo *repositories.VehicleCouplingRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		vehicleCouplings, err := repo.GetVehicleCouplings()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(vehicleCouplings)
	}
}
