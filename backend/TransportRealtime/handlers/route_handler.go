package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

func GetRoutesHandler(repo *repositories.RouteRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		routes, err := repo.GetRoutes()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(routes)
	}
}

func GetRouteHandler(repo *repositories.RouteRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		routeId := chi.URLParam(r, "route_id")

		route, err := repo.GetRoute(routeId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(route)
	}
}
