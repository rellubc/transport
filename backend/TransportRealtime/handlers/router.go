package handlers

import (
	"TransportRealtime/repositories"
	"net/http"

	"github.com/go-chi/chi/v5"
)

func RegisterRoutes(repos *repositories.Repositories) http.Handler {
	r := chi.NewRouter()

	// Agency
	r.Get("/api/agencies", GetAgenciesHandler(repos.Agency))
	r.Get("/api/agency/{agency_id}", GetAgencyHandler(repos.Agency))

	// Calendar
	r.Get("/api/calendars", GetCalendarsHandler(repos.Calendar))
	r.Get("/api/calendar/{service_id}", GetCalendarHandler(repos.Calendar))

	// Note
	r.Get("/api/notes", GetNotesHandler(repos.Note))
	r.Get("/api/note/{note_id}", GetNoteHandler(repos.Note))

	// Occupancy
	r.Get("/api/occupancies", GetOccupanciesHandler(repos.Occupancy))

	// Route
	r.Get("/api/routes", GetRoutesHandler(repos.Route))
	r.Get("/api/route/{route_id}", GetRouteHandler(repos.Route))

	// Shape
	r.Get("/api/shapes", GetShapesHandler(repos.Shape))

	// Stop
	r.Get("/api/stops", GetStopsHandler(repos.Stop))
	r.Get("/api/stop/{stop_id}", GetStopHandler(repos.Stop))

	// StopTime
	r.Get("/api/stop_times", GetStopTimesHandler(repos.StopTime))

	// Trip
	r.Get("/api/trips", GetTripsHandler(repos.Trip))
	r.Get("/api/trip/{trip_id}", GetTripHandler(repos.Trip))

	// Vehicle Boarding
	r.Get("/api/vehicle_boardings", GetVehicleBoardingsHandler(repos.VehicleBoarding))

	// Vehicle Category
	r.Get("/api/vehicle_categories", GetVehicleCategoriesHandler(repos.VehicleCategory))

	// Vehicle Coupling
	r.Get("/api/vehicle_couplings", GetVehicleCouplingsHandler(repos.VehicleCoupling))

	return r
}
