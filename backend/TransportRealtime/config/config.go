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

	NSWTrains TransportMode = "nswtrains"

	CBDSouthEast TransportMode = "lightrail/cbdandsoutheast"
	Newcastle    TransportMode = "lightrail/newcastle"
	Parramatta   TransportMode = "lightrail/parramatta"

	Buses TransportMode = "buses"

	SydneyFerries TransportMode = "ferries/sydneyferries"
	MFFerries     TransportMode = "ferries/MFF"
)

func BuildFeedURL(version string, feedType FeedType, mode TransportMode) string {
	return BaseUrl + "/" + version + "/gtfs/" + string(feedType) + "/" + string(mode)
}
