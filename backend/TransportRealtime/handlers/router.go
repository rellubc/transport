package handlers

import (
	"TransportRealtime/repositories"
	"net/http"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/cors"
)

func RegisterRoutes(repos *repositories.Repositories) http.Handler {
	r := chi.NewRouter()

	r.Use(cors.Handler(cors.Options{
		AllowedOrigins:   []string{"http://localhost:5173"},
		AllowedMethods:   []string{"GET"},
		AllowCredentials: false,
	}))

	// Agency
	r.Get("/api/sydney/agencies", GetAgenciesHandler(repos.Agency))
	r.Get("/api/sydney/agency/{agency_id}", GetAgencyHandler(repos.Agency))

	// Calendar
	r.Get("/api/sydney/calendars", GetCalendarsHandler(repos.Calendar))
	r.Get("/api/sydney/calendar/{service_id}", GetCalendarHandler(repos.Calendar))

	// Note
	r.Get("/api/sydney/notes", GetNotesHandler(repos.Note))
	r.Get("/api/sydney/note/{note_id}", GetNoteHandler(repos.Note))

	// Route
	r.Get("/api/sydney/routes", GetRoutesHandler(repos.Route))
	r.Get("/api/sydney/route/{route_id}", GetRouteHandler(repos.Route))

	// Shape
	r.Get("/api/sydney/shapes", GetShapesHandler(repos.Shape))

	// Stop
	r.Get("/api/sydney/stops", GetStopsHandler(repos.Stop))
	r.Get("/api/sydney/stops/{stop_id}", GetStopHandler(repos.Stop))

	// Static StopTime
	r.Get("/api/sydney/stop_times/static", GetStaticStopTimesHandler(repos.StopTime))
	r.Get("/api/sydney/stop_times/realtime", GetRealtimeStopTimesHandler(repos.StopTime))

	// Trip
	r.Get("/api/sydney/trips", GetTripsHandler(repos.Trip))
	r.Get("/api/sydney/trip/{trip_id}", GetTripHandler(repos.Trip))

	// Vehicle Boarding
	r.Get("/api/sydney/vehicle_boardings", GetVehicleBoardingsHandler(repos.VehicleBoarding))

	// Vehicle Category
	r.Get("/api/sydney/vehicle_categories", GetVehicleCategoriesHandler(repos.VehicleCategory))

	// Vehicle Coupling
	r.Get("/api/sydney/vehicle_couplings", GetVehicleCouplingsHandler(repos.VehicleCoupling))

	// Realtime
	// Trip Update
	r.Get("/api/sydney/trip_update", GetTripUpdateHandler(repos.TripUpdate))
	r.Get("/api/sydney/trip_update/{trip_id}/stop_time_updates", GetTripStopTimeUpdatesHandler(repos.TripUpdate))

	// Vehicle Position
	r.Get("/api/sydney/vehicles", GetVehiclePositionsHandler(repos.VehiclePosition))
	r.Get("/api/sydney/vehicles/{vehicle_id}", GetVehiclePositionHandler(repos.VehiclePosition))

	// Realtime stop times
	r.Get("/api/sydney/stops/{stop_id}/stop_times", GetStopStopTimesHandler(repos.StopTime))
	r.Get("/api/sydney/trips/{trip_id}/stop_times", GetTripStopTimesHandler(repos.StopTime))

	return r
}
