package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetVehicleCategoriesHandler(repo *repositories.VehicleCategoryRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		vehicleCategories, err := repo.GetVehicleCategories()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(vehicleCategories)
	}
}
