package config

const BaseUrl = "https://api.transport.nsw.gov.au"

const (
	V1 = "v1"
	V2 = "v2"
)

type FeedType string

const (
	VehiclePositions FeedType = "vehiclepos"
	TripUpdates      FeedType = "realtime"
)

type TransportMode string

const (
	Metro              TransportMode = "metro"
	SydneyTrains       TransportMode = "sydneytrains"
	InnerwestLightrail TransportMode = "lightrail/innerwest"
	NSWTrains          TransportMode = "nswtrains"
	Buses              TransportMode = "buses"
)

func BuildFeedURL(version string, feedType FeedType, mode TransportMode) string {
	return BaseUrl + "/" + version + "/gtfs/" + string(feedType) + "/" + string(mode)
}
