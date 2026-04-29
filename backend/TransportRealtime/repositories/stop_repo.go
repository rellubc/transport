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
	query := `
		SELECT stop_id, stop_code, stop_name, stop_desc, stop_lat, stop_lon,
		       stop_zone_id, stop_url, stop_location_type,
		       stop_parent_station, stop_timezone, stop_wheelchair_boarding,
		       stop_platform_code, route_type
		FROM stops
		ORDER BY stop_name
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
			&stop.StopCode,
			&stop.StopName,
			&stop.StopDescription,
			&stop.StopLat,
			&stop.StopLon,
			&stop.StopZoneId,
			&stop.StopUrl,
			&stop.StopLocationType,
			&stop.StopParentStation,
			&stop.StopTimezone,
			&stop.StopWheelchairBoarding,
			&stop.StopPlatformCode,
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
	row := r.DB.QueryRow(context.Background(), "SELECT * FROM stops WHERE stop_id = $1", stopId)

	var stop models.Stop
	err := row.Scan(
		&stop.StopId,
		&stop.StopCode,
		&stop.StopName,
		&stop.StopLat,
		&stop.StopLon,
		&stop.StopZoneId,
		&stop.StopUrl,
		&stop.StopLocationType,
		&stop.StopParentStation,
		&stop.StopTimezone,
		&stop.StopWheelchairBoarding,
		&stop.StopPlatformCode,
		&stop.RouteType,
	)

	return stop, err
}
