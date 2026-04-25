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

		{"NSWTrains TripUpdates", config.NSWTrains, config.V1, config.TripUpdates, realtime.FetchTripUpdatesV1, realtime.InsertTripUpdatesV1, nil},
		{"NSWTrains VehiclePositions", config.NSWTrains, config.V1, config.VehiclePositions, realtime.FetchVehiclePositionsV1, realtime.InsertVehiclePositionsV1, nil},

		{"CBDSouthEast Lightrail TripUpdates", config.CBDSouthEast, config.V1, config.TripUpdates, realtime.FetchTripUpdatesV1, realtime.InsertTripUpdatesV1, nil},
		{"CBDSouthEast Lightrail VehiclePositions", config.CBDSouthEast, config.V1, config.VehiclePositions, realtime.FetchVehiclePositionsV1, realtime.InsertVehiclePositionsV1, nil},
		{"Newcastle Lightrail TripUpdates", config.Newcastle, config.V1, config.TripUpdates, realtime.FetchTripUpdatesV1, realtime.InsertTripUpdatesV1, nil},
		{"Newcastle Lightrail VehiclePositions", config.Newcastle, config.V1, config.VehiclePositions, realtime.FetchVehiclePositionsV1, realtime.InsertVehiclePositionsV1, nil},
		{"Parramatta Lightrail TripUpdates", config.Parramatta, config.V1, config.TripUpdates, realtime.FetchTripUpdatesV1, realtime.InsertTripUpdatesV1, nil},
		{"Parramatta Lightrail VehiclePositions", config.Parramatta, config.V1, config.VehiclePositions, realtime.FetchVehiclePositionsV1, realtime.InsertVehiclePositionsV1, nil},

		{"Buses TripUpdates", config.Buses, config.V1, config.TripUpdates, realtime.FetchTripUpdatesV1, realtime.InsertTripUpdatesV1, nil},
		{"Buses VehiclePositions", config.Buses, config.V1, config.VehiclePositions, realtime.FetchVehiclePositionsV1, realtime.InsertVehiclePositionsV1, nil},

		{"SydneyFerries TripUpdates", config.SydneyFerries, config.V1, config.TripUpdates, realtime.FetchTripUpdatesV1, realtime.InsertTripUpdatesV1, nil},
		{"SydneyFerries VehiclePositions", config.SydneyFerries, config.V1, config.VehiclePositions, realtime.FetchVehiclePositionsV1, realtime.InsertVehiclePositionsV1, nil},
		{"MFFerries TripUpdates", config.MFFerries, config.V1, config.TripUpdates, realtime.FetchTripUpdatesV1, realtime.InsertTripUpdatesV1, nil},
		{"MFFerries VehiclePositions", config.MFFerries, config.V1, config.VehiclePositions, realtime.FetchVehiclePositionsV1, realtime.InsertVehiclePositionsV1, nil},
	}
}
