package tasks

import (
	"TransportRealtime/config"
	"TransportRealtime/realtime"
)

func GetRealtimeTasks() []FeedTask {
	return []FeedTask{
		{"Metro VehiclePositions", config.Metro, config.V2, config.VehiclePositions, realtime.FetchVehiclePositionsV2},
		{"Metro TripUpdates", config.Metro, config.V2, config.TripUpdates, realtime.FetchTripUpdatesV2},
		{"SydneyTrains VehiclePositions", config.SydneyTrains, config.V2, config.VehiclePositions, realtime.FetchVehiclePositionsV2},
		{"SydneyTrains TripUpdates", config.SydneyTrains, config.V2, config.TripUpdates, realtime.FetchTripUpdatesV2},
		{"Innerwest Lightrail VehiclePositions", config.InnerwestLightrail, config.V2, config.VehiclePositions, realtime.FetchVehiclePositionsV2},
		{"Innerwest Lightrail TripUpdates", config.InnerwestLightrail, config.V2, config.TripUpdates, realtime.FetchTripUpdatesV2},
	}
}
