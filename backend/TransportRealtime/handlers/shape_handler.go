package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
)

func GetShapesHandler(repo *repositories.ShapeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		shapeType := r.URL.Query().Get("shape_type")

		shapes, err := repo.GetShapes(shapeType)

		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(shapes)
	}
}
