package tasks

import (
	"TransportRealtime/config"
	"TransportRealtime/realtime"

	pb "TransportRealtime/proto"

	"github.com/jackc/pgx/v5/pgxpool"
)

type FeedTask struct {
	Name     string
	Mode     config.TransportMode
	Version  string
	Feed     config.FeedType
	FetchFn  func(config.TransportMode, string) (*pb.FeedMessage, error)
	InsertFn func(*pb.FeedMessage, *pgxpool.Pool, string) error
	DB       *pgxpool.Pool
}

func GetRealtimeTasks() []FeedTask {
	return []FeedTask{
		{"Metro TripUpdates", config.Metro, config.V2, config.TripUpdates, realtime.FetchTripUpdatesV2, realtime.InsertTripUpdatesV2, nil},
		{"Metro VehiclePositions", config.Metro, config.V2, config.VehiclePositions, realtime.FetchVehiclePositionsV2, realtime.InsertVehiclePositionsV2, nil},
		{"SydneyTrains TripUpdates", config.SydneyTrains, config.V2, config.TripUpdates, realtime.FetchTripUpdatesV2, realtime.InsertTripUpdatesV2, nil},
		{"SydneyTrains VehiclePositions", config.SydneyTrains, config.V2, config.VehiclePositions, realtime.FetchVehiclePositionsV2, realtime.InsertVehiclePositionsV2, nil},
		{"Innerwest Lightrail TripUpdates", config.InnerwestLightrail, config.V2, config.TripUpdates, realtime.FetchTripUpdatesV2, realtime.InsertTripUpdatesV2, nil},
		{"Innerwest Lightrail VehiclePositions", config.InnerwestLightrail, config.V2, config.VehiclePositions, realtime.FetchVehiclePositionsV2, realtime.InsertVehiclePositionsV2, nil},
	}
}
