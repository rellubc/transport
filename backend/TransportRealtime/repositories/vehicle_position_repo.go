package repositories

import (
	models "TransportRealtime/models/realtime"
	"context"
	"fmt"
	"log"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
)

type VehiclePositionRepository struct {
	DB *pgxpool.Pool
}

func NewVehiclePositionRepository(db *pgxpool.Pool) *VehiclePositionRepository {
	return &VehiclePositionRepository{DB: db}
}

func (r *VehiclePositionRepository) GetVehiclePositions(routeType *int) (map[int][]models.VehiclePosition, error) {
	var (
		rows pgx.Rows
		err  error
	)

	query := `
		SELECT
			vp.trip_id,
			vp.trip_route_id,
			r.route_short_name,
			vp.trip_schedule_relationship,
			vp.vehicle_id,
			vp.vehicle_label,
			vp.vehicle_model,
			vp.position_latitude,
			vp.position_longitude,
			vp.timestamp AT TIME ZONE 'Australia/Sydney' AS sydney_time,
			vp.congestion_level,
			vp.occupancy_status,
			vp.route_type
		FROM vehicle_positions vp
		JOIN routes r ON vp.trip_route_id = r.route_id
		%s
	`

	if routeType != nil {
		query = fmt.Sprintf(query, "WHERE ($1 IS NULL OR vp.route_type = $1) AND NOW() - vp.timestamp < INTERVAL '2 minutes' AND vp.trip_route_id NOT LIKE 'RTTA%'")
	} else {
		query = fmt.Sprintf(query, "WHERE NOW() - vp.timestamp < INTERVAL '2 minutes' AND vp.trip_route_id NOT LIKE 'RTTA%'")
	}

	rows, err = r.DB.Query(context.Background(), query)

	if err != nil {
		return nil, err
	}
	defer rows.Close()

	vpMap := make(map[int][]models.VehiclePosition)

	for rows.Next() {
		var vp models.VehiclePosition

		err := rows.Scan(
			&vp.TripId,
			&vp.RouteId,
			&vp.RouteShortName,
			&vp.ScheduleRelationship,
			&vp.VehicleId,
			&vp.VehicleLabel,
			&vp.VehicleModel,
			&vp.Latitude,
			&vp.Longitude,
			&vp.Timestamp,
			&vp.CongestionLevel,
			&vp.OccupancyStatus,
			&vp.RouteType,
		)
		if err != nil {
			return nil, err
		}

		var routeType int
		if vp.RouteType == nil {
			routeType = 9999
		} else {
			routeType = *vp.RouteType
		}

		vpMap[routeType] = append(vpMap[routeType], vp)
	}

	return vpMap, nil
}

func (r *VehiclePositionRepository) GetVehiclePosition(vehicleId string, tripId string) (models.VehiclePosition, error) {
	query := `
		SELECT
			vp.trip_id,
			vp.trip_route_id,
			r.route_short_name,
			vp.trip_schedule_relationship,
			vp.vehicle_id,
			vp.vehicle_label,
			vp.vehicle_model,
			vp.position_latitude,
			vp.position_longitude,
			vp.timestamp AT TIME ZONE 'Australia/Sydney' AS sydney_time,
			vp.congestion_level,
			vp.occupancy_status,
			vp.route_type
		FROM vehicle_positions vp 
		JOIN routes r ON vp.trip_route_id = r.route_id
		WHERE NOW() - vp.timestamp < INTERVAL '2 minutes'
	`

	log.Println(vehicleId, tripId)

	var args []any
	if vehicleId != "" {
		query += " AND vp.vehicle_id = $1"
		args = append(args, vehicleId)
	} else if tripId != "" {
		query += " AND vp.trip_id = $1"
		args = append(args, tripId)
	}

	log.Println(query, args)

	row := r.DB.QueryRow(context.Background(), query, args...)

	var vp models.VehiclePosition

	err := row.Scan(
		&vp.TripId,
		&vp.RouteId,
		&vp.RouteShortName,
		&vp.ScheduleRelationship,
		&vp.VehicleId,
		&vp.VehicleLabel,
		&vp.VehicleModel,
		&vp.Latitude,
		&vp.Longitude,
		&vp.Timestamp,
		&vp.CongestionLevel,
		&vp.OccupancyStatus,
		&vp.RouteType,
	)
	if err != nil {
		return models.VehiclePosition{}, err
	}

	return vp, err
}
