package tasks

import (
	"TransportRealtime/config"
	"TransportRealtime/ingest"
	"context"

	pb "TransportRealtime/proto"

	"github.com/jackc/pgx/v5/pgxpool"
)

type FeedTask struct {
	Name          string
	FeedVersion   config.FeedVersion
	FeedType      config.FeedType
	TransportMode config.TransportMode
	FetchFn       func(context.Context, string, config.FeedVersion, config.TransportMode) (*pb.FeedMessage, error)
	InsertFn      func(context.Context, *pb.FeedMessage, *pgxpool.Pool) error
}

func RealtimeTasks() []FeedTask {
	return []FeedTask{
		{
			Name:          "Metro TripUpdates",
			FeedVersion:   config.V2,
			FeedType:      config.TripUpdates,
			TransportMode: config.Metro,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "Metro VehiclePositions",
			FeedVersion:   config.V2,
			FeedType:      config.VehiclePositions,
			TransportMode: config.Metro,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "SydneyTrains TripUpdates",
			FeedVersion:   config.V2,
			FeedType:      config.TripUpdates,
			TransportMode: config.SydneyTrains,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "SydneyTrains VehiclePositions",
			FeedVersion:   config.V2,
			FeedType:      config.VehiclePositions,
			TransportMode: config.SydneyTrains,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "Innerwest Lightrail TripUpdates",
			FeedVersion:   config.V2,
			FeedType:      config.TripUpdates,
			TransportMode: config.InnerwestLightrail,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "Innerwest Lightrail VehiclePositions",
			FeedVersion:   config.V2,
			FeedType:      config.VehiclePositions,
			TransportMode: config.InnerwestLightrail,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "NSWTrains TripUpdates",
			FeedVersion:   config.V1,
			FeedType:      config.TripUpdates,
			TransportMode: config.NSWTrains,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "NSWTrains VehiclePositions",
			FeedVersion:   config.V1,
			FeedType:      config.VehiclePositions,
			TransportMode: config.NSWTrains,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "CBDSouthEast Lightrail TripUpdates",
			FeedVersion:   config.V1,
			FeedType:      config.TripUpdates,
			TransportMode: config.CBDSouthEast,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "CBDSouthEast Lightrail VehiclePositions",
			FeedVersion:   config.V1,
			FeedType:      config.VehiclePositions,
			TransportMode: config.CBDSouthEast,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "Newcastle Lightrail TripUpdates",
			FeedVersion:   config.V1,
			FeedType:      config.TripUpdates,
			TransportMode: config.Newcastle,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "Newcastle Lightrail VehiclePositions",
			FeedVersion:   config.V1,
			FeedType:      config.VehiclePositions,
			TransportMode: config.Newcastle,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "Parramatta Lightrail TripUpdates",
			FeedVersion:   config.V1,
			FeedType:      config.TripUpdates,
			TransportMode: config.Parramatta,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "Parramatta Lightrail VehiclePositions",
			FeedVersion:   config.V1,
			FeedType:      config.VehiclePositions,
			TransportMode: config.Parramatta,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "SydneyFerries TripUpdates",
			FeedVersion:   config.V1,
			FeedType:      config.TripUpdates,
			TransportMode: config.SydneyFerries,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "SydneyFerries VehiclePositions",
			FeedVersion:   config.V1,
			FeedType:      config.VehiclePositions,
			TransportMode: config.SydneyFerries,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
		{
			Name:          "MFFerries TripUpdates",
			FeedVersion:   config.V1,
			FeedType:      config.TripUpdates,
			TransportMode: config.MFFerries,
			FetchFn:       ingest.FetchTripUpdates,
			InsertFn:      ingest.InsertTripUpdates,
		},
		{
			Name:          "MFFerries VehiclePositions",
			FeedVersion:   config.V1,
			FeedType:      config.VehiclePositions,
			TransportMode: config.MFFerries,
			FetchFn:       ingest.FetchVehiclePositions,
			InsertFn:      ingest.InsertVehiclePositions,
		},
	}
}
