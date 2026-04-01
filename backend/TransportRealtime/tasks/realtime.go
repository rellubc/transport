package tasks

import (
	"log"
	"time"

	"TransportRealtime/config"

	pb "TransportRealtime/proto"
)

type FeedTask struct {
	Name    string
	Mode    config.TransportMode
	Version string
	Feed    config.FeedType
	Fn      func(config.TransportMode, string) (*pb.FeedMessage, error)
}

func (t FeedTask) Start(apiKey string, delay time.Duration, interval time.Duration) {
	time.Sleep(delay)

	for {
		feed, err := t.Fn(t.Mode, apiKey)
		if err != nil {
			log.Printf("Error fetching %s: %v", t.Name, err)
		} else {
			log.Printf("[%s] Fetched %d entities for %s\n", time.Now().Format(time.RFC3339), len(feed.Entity), t.Name)
		}

		time.Sleep(interval)
	}
}
