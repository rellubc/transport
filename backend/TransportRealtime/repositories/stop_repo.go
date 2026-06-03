package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type StopRepository struct {
	DB *pgxpool.Pool
}

func NewStopRepository(db *pgxpool.Pool) *StopRepository {
	return &StopRepository{DB: db}
}

func (r *StopRepository) GetStops() (map[int][]models.Stop, error) {
	// query := `
	// 	SELECT stop_id, stop_name, stop_lat, stop_lon, stop_parent_station, stop_wheelchair_boarding, route_type
	// 	FROM stops
	// 	ORDER BY stop_name
	// `

	query := `
		SELECT s.stop_id, s.stop_name, s.stop_lat, s.stop_lon, s.stop_parent_station, s.stop_wheelchair_boarding, s.route_type
		FROM stops s
		WHERE
			(
				s.stop_parent_station IS NOT NULL
				AND EXISTS (
					SELECT 1
					FROM stop_times st
					WHERE st.stop_id = s.stop_id
				)
			)
			OR
			(
				s.stop_parent_station IS NULL
				AND EXISTS (
					SELECT 1
					FROM stops child
					JOIN stop_times st
						ON st.stop_id = child.stop_id
					WHERE child.stop_parent_station = s.stop_id
				)
			)
		ORDER BY s.stop_name;
	`

	rows, err := r.DB.Query(context.Background(), query)

	if err != nil {
		return nil, err
	}
	defer rows.Close()

	stopMap := make(map[int][]models.Stop)

	for rows.Next() {
		var stop models.Stop
		err := rows.Scan(
			&stop.StopId,
			&stop.StopName,
			&stop.StopLat,
			&stop.StopLon,
			&stop.StopParentStation,
			&stop.StopWheelchairBoarding,
			&stop.RouteType,
		)

		if err != nil {
			return nil, err
		}

		var routeType int
		if stop.RouteType == nil {
			routeType = 9999
		} else {
			routeType = *stop.RouteType
		}

		stopMap[routeType] = append(stopMap[routeType], stop)
	}

	return stopMap, nil
}

func (r *StopRepository) GetStop(stopId string) (models.Stop, error) {
	query := `
		SELECT stop_id, stop_name, stop_lat, stop_lon, stop_parent_station, stop_wheelchair_boarding, route_type
		FROM stops
		WHERE stop_id = $1
	`
	args := []any{stopId}

	row := r.DB.QueryRow(context.Background(), query, args...)

	var stop models.Stop
	err := row.Scan(
		&stop.StopId,
		&stop.StopName,
		&stop.StopLat,
		&stop.StopLon,
		&stop.StopParentStation,
		&stop.StopWheelchairBoarding,
		&stop.RouteType,
	)

	return stop, err
}
