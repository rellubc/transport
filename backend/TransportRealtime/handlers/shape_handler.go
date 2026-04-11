package handlers

import (
	models "TransportRealtime/models/static"
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"
	"strings"
)

func GetShapesHandler(repo *repositories.ShapeRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		lines := r.URL.Query()["lines"]

		for i := range lines {
			lines[i] = strings.ToLower(lines[i])
		}

		var shapes map[string][]models.ShapeDetails
		var err error
		if len(lines) == 0 {
			shapes, err = repo.GetShapes([]string{})
		} else {
			shapes, err = repo.GetShapes(lines)
		}

		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(shapes)
	}
}

// package handlers

// import (
// 	models "TransportRealtime/models/static"
// 	"TransportRealtime/repositories"
// 	"encoding/json"
// 	"net/http"
// 	"strings"
// )

// func GetShapesHandler(repo *repositories.ShapeRepository) http.HandlerFunc {
// 	return func(w http.ResponseWriter, r *http.Request) {
// 		mode := strings.ToLower(r.URL.Query().Get("mode"))

// 		var shapes map[string][]models.Shape
// 		var err error
// 		if mode != "" {
// 			shapes, err = repo.GetShapes(mode)
// 		} else {
// 			shapes, err = repo.GetShapes("")
// 		}

// 		if err != nil {
// 			http.Error(w, err.Error(), http.StatusInternalServerError)
// 			return
// 		}

// 		w.Header().Set("Content-Type", "application/json")
// 		json.NewEncoder(w).Encode(shapes)
// 	}
// }
