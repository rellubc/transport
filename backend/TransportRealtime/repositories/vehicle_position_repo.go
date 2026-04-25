package repositories

import (
	models "TransportRealtime/models/realtime"
	"context"
	"strings"

	"TransportRealtime/constants"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
)

type VehiclePositionRepository struct {
	DB *pgxpool.Pool
}

func NewVehiclePositionRepository(db *pgxpool.Pool) *VehiclePositionRepository {
	return &VehiclePositionRepository{DB: db}
}

func (r *VehiclePositionRepository) GetVehiclePositions(mode string) (map[string][]models.VehiclePosition, error) {
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
			vp.mode
		FROM vehicle_positions vp
		WHERE ($1 = '' OR vp.mode LIKE $1) AND NOW() - vp.timestamp < INTERVAL '2 minutes'
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
			vp.mode
	`

	rows, err = r.DB.Query(context.Background(), query, mode)

	if err != nil {
		return nil, err
	}
	defer rows.Close()

	vpMap := make(map[string][]models.VehiclePosition)

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
			&vp.Mode,
		)
		if err != nil {
			return nil, err
		}

		key := vp.Mode

		if vp.RouteId != "" {
			for k, v := range constants.RoutesLookup {
				if vp.Mode == "metro" && strings.Split(vp.RouteId, "_")[1] == k {
					key = v
					break
				} else if vp.Mode == "sydneytrains" && strings.Split(vp.RouteId, "_")[0] == k {
					key = v
					break
				} else if vp.Mode == "nswtrains" && strings.Split(vp.RouteId, ".")[2] == k {
					key = v
					break
				} else if vp.Mode == "buses" && strings.Split(vp.RouteId, "_")[1] == k {
					key = v
					break
				} else if vp.Mode == "lightrail/cbdandsoutheast" && strings.Split(vp.RouteId, "_")[1] == k {
					key = v
					break
				} else if vp.Mode == "lightrail/innerwest" && strings.Split(vp.RouteId, "-")[0] == k {
					key = v
					break
				} else if vp.Mode == "lightrail/parramatta" && strings.Split(vp.RouteId, "_")[1] == k {
					key = v
					break
				} else if vp.Mode == "ferries/sydneyferries" && strings.Split(vp.RouteId, "-")[1] == k {
					key = v
					break
				} else if vp.Mode == "ferries/MFF" && strings.Split(vp.RouteId, "-")[0] == k {
					key = v
					break
				}
			}
		}

		vpMap[key] = append(vpMap[key], vp)
	}

	return vpMap, nil
}

func (r *VehiclePositionRepository) GetVehiclePosition(vehicleId string) (models.VehiclePosition, error) {
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
			vp.mode
		FROM vehicle_positions vp
		WHERE ($1 = '' OR vp.mode LIKE $1) AND NOW() - vp.timestamp < INTERVAL '2 minutes'
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
			vp.mode
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
		&vp.Mode,
	)
	if err != nil {
		return models.VehiclePosition{}, err
	}

	return vp, err
}
