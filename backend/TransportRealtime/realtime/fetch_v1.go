package realtime

import (
	"TransportRealtime/config"
	pb "TransportRealtime/proto"
)

func FetchVehiclePositionsV1(mode config.TransportMode, apiKey string) (*pb.FeedMessage, error) {
	url := config.BuildFeedURL(config.V1, config.VehiclePositions, mode)
	return fetchProtobuf(url, apiKey)
}

func FetchTripUpdatesV1(mode config.TransportMode, apiKey string) (*pb.FeedMessage, error) {
	url := config.BuildFeedURL(config.V1, config.TripUpdates, mode)
	return fetchProtobuf(url, apiKey)
}
