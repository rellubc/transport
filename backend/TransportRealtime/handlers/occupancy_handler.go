package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetOccupanciesHandler(repo *repositories.OccupancyRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		occupancies, err := repo.GetOccupancies()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(occupancies)
	}
}
