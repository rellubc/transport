package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

func GetCalendarsHandler(repo *repositories.CalendarRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		calendars, err := repo.GetCalendars()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(calendars)
	}
}

func GetCalendarHandler(repo *repositories.CalendarRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		serviceId := chi.URLParam(r, "service_id")

		calendar, err := repo.GetCalendar(serviceId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(calendar)
	}
}
