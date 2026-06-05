package ingest

import (
	"TransportRealtime/config"
	"TransportRealtime/constants"
	pb "TransportRealtime/proto"
	"context"
	"fmt"
	"io"
	"net/http"
	"time"

	"google.golang.org/protobuf/proto"
)

var httpClient = &http.Client{
	Timeout: 10 * time.Second,
}

func FetchVehiclePositions(ctx context.Context, apiKey string, version config.FeedVersion, mode config.TransportMode) (*pb.FeedMessage, error) {
	url := BuildFeedURL(config.VehiclePositions, version, mode)
	return FetchProtobufData(ctx, url, apiKey)
}
func FetchTripUpdates(ctx context.Context, apiKey string, version config.FeedVersion, mode config.TransportMode) (*pb.FeedMessage, error) {
	url := BuildFeedURL(config.TripUpdates, version, mode)
	return FetchProtobufData(ctx, url, apiKey)
}

func BuildFeedURL(feedType config.FeedType, version config.FeedVersion, mode config.TransportMode) string {
	return constants.BaseUrl + "/" + string(version) + "/gtfs/" + string(feedType) + "/" + string(mode)
}

func FetchProtobufData(ctx context.Context, url, apiKey string) (*pb.FeedMessage, error) {
	data, err := ProtobufDataRequest(ctx, url, apiKey)
	if err != nil {
		return nil, err
	}

	feed := &pb.FeedMessage{}
	if err := proto.Unmarshal(data, feed); err != nil {
		return nil, err
	}

	return feed, nil
}

func ProtobufDataRequest(ctx context.Context, url, apiKey string) ([]byte, error) {
	req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
	if err != nil {
		return nil, err
	}

	req.Header.Add("Authorization", fmt.Sprintf("apikey %s", apiKey))
	req.Header.Add("Accept", "application/x-protobuf")

	res, err := httpClient.Do(req)
	if err != nil {
		return nil, err
	}
	defer res.Body.Close()

	if res.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(res.Body)
		return nil, fmt.Errorf("bad response %d: %s", res.StatusCode, string(body))
	}

	return io.ReadAll(res.Body)
}
