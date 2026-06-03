package tasks

import (
	"TransportRealtime/constants"
	"context"
	"log"
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
)

func (t FeedTask) StartRealtimeTask(ctx context.Context, apiKey string, delay time.Duration, db *pgxpool.Pool) {
	select {
	case <-time.After(delay):
	case <-ctx.Done():
		return
	}

	ticker := time.NewTicker(constants.FeedInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
		}

		feed, err := t.FetchFn(ctx, apiKey, t.FeedVersion, t.TransportMode)

		if err != nil {
			log.Printf("Error fetching %s: %v", t.Name, err)
		} else {
			log.Printf("[%s] Fetched %d entities for %s\n", time.Now().Format(time.RFC3339), len(feed.Entity), t.Name)

			err := t.InsertFn(ctx, feed, db)
			if err != nil {
				log.Printf("Error inserting %s: %v", t.Name, err)
			}
		}
	}
}
