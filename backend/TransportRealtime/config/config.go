package config

import (
	"fmt"
	"os"
)

type Config struct {
	APIKey string
	DBUrl  string
}

func Load() (*Config, error) {
	apiKey := os.Getenv("API_KEY")
	if apiKey == "" {
		return nil, fmt.Errorf("API_KEY not set")
	}

	dbURL := os.Getenv("DATABASE_URL")
	if dbURL == "" {
		return nil, fmt.Errorf("DATABASE_URL not set")
	}

	return &Config{
		APIKey: apiKey,
		DBUrl:  dbURL,
	}, nil
}

const BaseUrl = "https://api.transport.nsw.gov.au"

type FeedVersion string

const (
	V1 FeedVersion = "v1"
	V2 FeedVersion = "v2"
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
