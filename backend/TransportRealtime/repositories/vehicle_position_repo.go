package repositories

import (
	models "TransportRealtime/models/realtime"
	"context"
	"fmt"

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
			vp.trip_schedule_relationship,
			vp.vehicle_id,
			vp.vehicle_label,
			vp.vehicle_model,
			vp.position_latitude,
			vp.position_longitude,
			vp.stop_id,
			vp.timestamp AT TIME ZONE 'Australia/Sydney' AS sydney_time,
			vp.congestion_level,
			vp.occupancy_status,
			vp.route_type
		FROM vehicle_positions vp
		%s
		GROUP BY
			vp.trip_id,
			vp.trip_route_id,
			vp.trip_schedule_relationship,
			vp.vehicle_id,
			vp.vehicle_label,
			vp.vehicle_model,
			vp.position_latitude,
			vp.position_longitude,
			vp.stop_id,
			sydney_time,
			vp.congestion_level,
			vp.occupancy_status,
			vp.route_type
	`

	if routeType != nil {
		query = fmt.Sprintf(query, "WHERE ($1 IS NULL OR vp.route_type = $1) AND NOW() - vp.timestamp < INTERVAL '2 minutes'")
	} else {
		query = fmt.Sprintf(query, "WHERE NOW() - vp.timestamp < INTERVAL '2 minutes'")
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
			&vp.ScheduleRelationship,
			&vp.VehicleId,
			&vp.VehicleLabel,
			&vp.VehicleModel,
			&vp.Latitude,
			&vp.Longitude,
			&vp.StopId,
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

func (r *VehiclePositionRepository) GetVehiclePosition(vehicleId string) (models.VehiclePosition, error) {
	query := `
		SELECT
			trip_id,
			trip_route_id,
			trip_schedule_relationship,
			vehicle_id,
			vehicle_label,
			vehicle_model,
			position_latitude,
			position_longitude,
			stop_id,
			timestamp AT TIME ZONE 'Australia/Sydney' AS sydney_time,
			congestion_level,
			occupancy_status,
			route_type
		FROM vehicle_positions
		WHERE vehicle_id = $1 AND NOW() - timestamp < INTERVAL '2 minutes'
	`

	row := r.DB.QueryRow(context.Background(), query, vehicleId)

	var vp models.VehiclePosition

	err := row.Scan(
		&vp.TripId,
		&vp.RouteId,
		&vp.ScheduleRelationship,
		&vp.VehicleId,
		&vp.VehicleLabel,
		&vp.VehicleModel,
		&vp.Latitude,
		&vp.Longitude,
		&vp.StopId,
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
