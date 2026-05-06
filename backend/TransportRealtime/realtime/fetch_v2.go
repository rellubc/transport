package realtime

import (
	"TransportRealtime/config"
	pb "TransportRealtime/proto"
)

func FetchVehiclePositionsV2(mode config.TransportMode, apiKey string) (*pb.FeedMessage, error) {
	url := config.BuildFeedURL(config.V2, config.VehiclePositions, mode)
	return fetchProtobuf(url, apiKey)
}

func FetchTripUpdatesV2(mode config.TransportMode, apiKey string) (*pb.FeedMessage, error) {
	url := config.BuildFeedURL(config.V2, config.TripUpdates, mode)
	return fetchProtobuf(url, apiKey)
}
