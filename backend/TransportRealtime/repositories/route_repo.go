package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type RouteRepository struct {
	DB *pgxpool.Pool
}

func NewRouteRepository(db *pgxpool.Pool) *RouteRepository {
	return &RouteRepository{DB: db}
}

func (r *RouteRepository) GetRoutes() ([]models.Route, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM routes")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var routes []models.Route
	for rows.Next() {
		var route models.Route
		err := rows.Scan(
			&route.RouteId,
			&route.AgencyId,
			&route.RouteShortName,
			&route.RouteLongName,
			&route.RouteDescription,
			&route.RouteType,
			&route.RouteColor,
			&route.RouteTextColor,
			&route.RouteUrl,
		)

		if err != nil {
			return nil, err
		}

		routes = append(routes, route)
	}

	return routes, nil
}

func (r *RouteRepository) GetRoute(routeId string) (models.Route, error) {
	row := r.DB.QueryRow(context.Background(), "SELECT * FROM routes WHERE route_id = $1", routeId)

	var route models.Route
	err := row.Scan(
		&route.RouteId,
		&route.AgencyId,
		&route.RouteShortName,
		&route.RouteLongName,
		&route.RouteDescription,
		&route.RouteType,
		&route.RouteColor,
		&route.RouteTextColor,
		&route.RouteUrl,
	)

	return route, err
}
