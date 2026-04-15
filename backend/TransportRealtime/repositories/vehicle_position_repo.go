package repositories

import (
	models "TransportRealtime/models/realtime"
	"context"
	"encoding/json"
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
			vp.mode,
			json_agg(
				json_build_object(
					'vehicleId', c.vehicle_id,
					'positionInConsist', c.position_in_consist,
					'occupancyStatus', c.occupancy_status
				)
			) FILTER (WHERE c.vehicle_id IS NOT NULL) AS consist
		FROM vehicle_positions vp
		JOIN consist c
			ON vp.vehicle_id = c.vehicle_id
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
		var consistJSON []byte

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
			&consistJSON,
		)
		if err != nil {
			return nil, err
		}

		err = json.Unmarshal(consistJSON, &vp.Consist)
		if err != nil {
			return nil, err
		}

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
			vp.mode,
			json_agg(
				json_build_object(
					'vehicleId', c.vehicle_id,
					'positionInConsist', c.position_in_consist,
					'occupancyStatus', c.occupancy_status
				)
			) FILTER (WHERE c.vehicle_id IS NOT NULL) AS consist
		FROM vehicle_positions vp
		JOIN consist c
			ON vp.vehicle_id = c.vehicle_id
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
	var consistJSON []byte

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
		&consistJSON,
	)
	if err != nil {
		return models.VehiclePosition{}, err
	}

	err = json.Unmarshal(consistJSON, &vp.Consist)
	if err != nil {
		return models.VehiclePosition{}, err
	}

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
