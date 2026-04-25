package tasks

import (
	"log"
	"time"
)

func (t FeedTask) Start(apiKey string, delay time.Duration, interval time.Duration) {
	time.Sleep(delay)

	for {
		feed, err := t.FetchFn(t.Mode, apiKey)

		if err != nil {
			log.Printf("Error fetching %s: %v", t.Name, err)
		} else {
			log.Printf("[%s] Fetched %d entities for %s\n", time.Now().Format(time.RFC3339), len(feed.Entity), t.Name)

			err := t.InsertFn(feed, t.DB, string(t.Mode))
			if err != nil {
				log.Printf("Error inserting %s: %v", t.Name, err)
			}
		}

		time.Sleep(interval)
	}
}
