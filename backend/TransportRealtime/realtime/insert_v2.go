package realtime

import (
	"context"
	"log"
	"time"

	pb "TransportRealtime/proto"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
	"google.golang.org/protobuf/proto"
)

func InsertVehiclePositionsV2(feed *pb.FeedMessage, db *pgxpool.Pool, mode string) error {
	ctx := context.Background()

	batch := &pgx.Batch{}

	for _, entity := range feed.Entity {
		vehicle := entity.GetVehicle()
		if vehicle == nil {
			continue
		}

		position := vehicle.GetPosition()
		if position == nil {
			continue
		}

		ext := proto.GetExtension(vehicle.GetVehicle(), pb.E_TfnswVehicleDescriptor)
		tfnsw := ext.(*pb.TfnswVehicleDescriptor)

		batch.Queue(`
			INSERT INTO vehicle_positions (
				trip_id, trip_route_id, trip_schedule_relationship,
				vehicle_id, vehicle_label, vehicle_model,
				position_latitude, position_longitude, position_geom,
				stop_id, timestamp, congestion_level, occupancy_status, mode
			) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,ST_SetSRID(ST_MakePoint($9,$10),4326),$11,$12,$13,$14,$15)
			ON CONFLICT (vehicle_id) DO UPDATE SET
				trip_id=EXCLUDED.trip_id,
				trip_route_id=EXCLUDED.trip_route_id,
				trip_schedule_relationship=EXCLUDED.trip_schedule_relationship,
				vehicle_label=EXCLUDED.vehicle_label,
				vehicle_model=EXCLUDED.vehicle_model,
				position_latitude=EXCLUDED.position_latitude,
				position_longitude=EXCLUDED.position_longitude,
				position_geom=EXCLUDED.position_geom,
				stop_id=EXCLUDED.stop_id,
				timestamp=EXCLUDED.timestamp,
				congestion_level=EXCLUDED.congestion_level,
				occupancy_status=EXCLUDED.occupancy_status,
				mode=EXCLUDED.mode
		`,
			vehicle.GetTrip().GetTripId(),
			vehicle.GetTrip().GetRouteId(),
			vehicle.GetTrip().GetScheduleRelationship().String(),
			vehicle.GetVehicle().GetId(),
			vehicle.GetVehicle().GetLabel(),
			tfnsw.GetVehicleModel(),
			vehicle.GetPosition().GetLatitude(),
			vehicle.GetPosition().GetLongitude(),
			vehicle.GetPosition().GetLongitude(),
			vehicle.GetPosition().GetLatitude(),
			vehicle.GetStopId(),
			time.Unix(int64(vehicle.GetTimestamp()), 0).UTC(),
			vehicle.GetCongestionLevel().String(),
			vehicle.GetOccupancyStatus().String(),
			mode,
		)

		ext = proto.GetExtension(vehicle, pb.E_Consist)
		consist := ext.([]*pb.CarriageDescriptor)

		for _, c := range consist {
			batch.Queue(`
				INSERT INTO consist (vehicle_id, position_in_consist, occupancy_status)
				VALUES ($1,$2,$3)
				ON CONFLICT (vehicle_id, position_in_consist) DO UPDATE SET
					occupancy_status=EXCLUDED.occupancy_status
			`, vehicle.GetVehicle().GetId(), c.GetPositionInConsist(), c.GetOccupancyStatus().String())
		}
	}

	br := db.SendBatch(ctx, batch)

	for i := 0; i < batch.Len(); i++ {
		_, err := br.Exec()
		if err != nil {
			log.Printf("Failed to execute batch query: %v", err)
		}
	}

	br.Close()
	return nil
}

func InsertTripUpdatesV2(feed *pb.FeedMessage, db *pgxpool.Pool, mode string) error {
	ctx := context.Background()

	batch := &pgx.Batch{}

	for _, entity := range feed.Entity {
		tripUpdate := entity.GetTripUpdate()
		if tripUpdate == nil {
			continue
		}

		ext := proto.GetExtension(tripUpdate.GetVehicle(), pb.E_TfnswVehicleDescriptor)
		tfnsw := ext.(*pb.TfnswVehicleDescriptor)

		batch.Queue(`
			INSERT INTO trip_updates (
				trip_id, trip_route_id, trip_schedule_relationship,
				vehicle_id, vehicle_label, vehicle_model,
				timestamp, mode
			) VALUES ($1,$2,$3,$4,$5,$6,$7,$8)
			ON CONFLICT (trip_id) DO UPDATE SET
				trip_id=EXCLUDED.trip_id,
				trip_route_id=EXCLUDED.trip_route_id,
				trip_schedule_relationship=EXCLUDED.trip_schedule_relationship,
				vehicle_label=EXCLUDED.vehicle_label,
				vehicle_model=EXCLUDED.vehicle_model,
				timestamp=EXCLUDED.timestamp,
				mode=EXCLUDED.mode
		`,
			tripUpdate.GetTrip().GetTripId(),
			tripUpdate.GetTrip().GetRouteId(),
			tripUpdate.GetTrip().GetScheduleRelationship().String(),
			tripUpdate.GetVehicle().GetId(),
			tripUpdate.GetVehicle().GetLabel(),
			tfnsw.GetVehicleModel(),
			time.Unix(int64(tripUpdate.GetTimestamp()), 0).UTC(),
			mode,
		)

		for _, stu := range tripUpdate.StopTimeUpdate {
			batch.Queue(`
				INSERT INTO stop_time_updates (trip_id, stop_id, stop_arrival_time, stop_departure_time)
				VALUES ($1,$2,$3,$4)
				ON CONFLICT (trip_id, stop_id) DO UPDATE SET
					stop_arrival_time=EXCLUDED.stop_arrival_time,
					stop_departure_time=EXCLUDED.stop_departure_time
			`, tripUpdate.GetTrip().GetTripId(), stu.GetStopId(), time.Unix(int64(stu.GetArrival().GetTime()), 0).UTC(), time.Unix(int64(stu.GetDeparture().GetTime()), 0).UTC())

			ext := proto.GetExtension(stu, pb.E_CarriageSeqPredictiveOccupancy)
			carriageOccupancies := ext.([]*pb.CarriageDescriptor)

			for _, co := range carriageOccupancies {
				batch.Queue(`
					INSERT INTO carriage_sequence_predictive_occupancies (trip_id, stop_id, position_in_consist, departure_occupancy_status)
					VALUES ($1,$2,$3,$4)
					ON CONFLICT (trip_id, stop_id, position_in_consist) DO UPDATE SET
						departure_occupancy_status=EXCLUDED.departure_occupancy_status
				`, tripUpdate.GetTrip().GetTripId(), stu.GetStopId(), co.GetPositionInConsist(), co.GetOccupancyStatus().String())
			}
		}
	}

	br := db.SendBatch(ctx, batch)

	for i := 0; i < batch.Len(); i++ {
		_, err := br.Exec()
		if err != nil {
			log.Printf("Failed to execute batch query: %v", err)
		}
	}

	br.Close()

	return nil
}
