package repositories

import "github.com/jackc/pgx/v5/pgxpool"

type Repositories struct {
	Agency          *AgencyRepository
	Calendar        *CalendarRepository
	Note            *NoteRepository
	Occupancy       *OccupancyRepository
	Route           *RouteRepository
	Shape           *ShapeRepository
	Stop            *StopRepository
	StopTime        *StopTimeRepository
	Trip            *TripRepository
	VehicleBoarding *VehicleBoardingRepository
	VehicleCategory *VehicleCategoryRepository
	VehicleCoupling *VehicleCouplingRepository

	TripUpdate        *TripUpdateRepository
	VehiclePosition   *VehiclePositionRepository
	CarriageOccupancy *CarriageOccupancyRepository
}

func NewRepositories(db *pgxpool.Pool) *Repositories {
	return &Repositories{
		Agency:          NewAgencyRepository(db),
		Calendar:        NewCalendarRepository(db),
		Note:            NewNoteRepository(db),
		Occupancy:       NewOccupancyRepository(db),
		Route:           NewRouteRepository(db),
		Shape:           NewShapeRepository(db),
		Stop:            NewStopRepository(db),
		StopTime:        NewStopTimeRepository(db),
		Trip:            NewTripRepository(db),
		VehicleBoarding: NewVehicleBoardingRepository(db),
		VehicleCategory: NewVehicleCategoryRepository(db),
		VehicleCoupling: NewVehicleCouplingRepository(db),

		TripUpdate:        NewTripUpdateRepository(db),
		VehiclePosition:   NewVehiclePositionRepository(db),
		CarriageOccupancy: NewCarriageOccupancyRepository(db),
	}
}
