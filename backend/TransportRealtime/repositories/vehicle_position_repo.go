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
			mode
		FROM vehicle_positions
		WHERE ($1 = '' OR mode LIKE $1) AND NOW() - timestamp < INTERVAL '2 minutes'
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

		consists, err := GetVehicleConsist(r, *vp.VehicleId)
		if err != nil {
			return nil, err
		}
		vp.Consist = consists

		key := vp.Mode

		if vp.RouteId != "" {
			for k, v := range constants.RoutesLookup {
				if strings.Contains(vp.RouteId, k) {
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
			occupancy_status
		FROM vehicle_positions
		WHERE trip_route_id LIKE $1 AND NOW() - timestamp < INTERVAL '2 minutes'
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
	)
	if err != nil {
		return models.VehiclePosition{}, err
	}

	consists, err := GetVehicleConsist(r, *vp.VehicleId)
	if err != nil {
		return models.VehiclePosition{}, err
	}
	vp.Consist = consists

	return vp, err
}

func GetVehicleConsist(r *VehiclePositionRepository, vehicleId string) ([]models.Consist, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM consist WHERE vehicle_id = $1", vehicleId)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var consists []models.Consist
	for rows.Next() {
		var consist models.Consist
		err := rows.Scan(
			&consist.VehicleId,
			&consist.PositionInConsist,
			&consist.OccupancyStatus,
		)
		if err != nil {
			return nil, err
		}

		consists = append(consists, consist)
	}

	return consists, nil
}
