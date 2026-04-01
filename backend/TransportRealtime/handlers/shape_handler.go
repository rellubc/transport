package handlers

import (
	"TransportRealtime/models"
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetShapesHandler(repo *repositories.ShapeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		mode := r.URL.Query().Get("mode")

		var shapes []models.Shape
		var err error

		if mode != "" {
			shapes, err = repo.GetShapes(mode)
		} else {
			shapes, err = repo.GetShapes("")
		}

		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(shapes)
	}
}
