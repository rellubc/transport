package tasks

import (
	"context"
	"log"
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
)

func StartMaterialisedViewRefresh(db *pgxpool.Pool) {
	ticker := time.NewTicker(1 * time.Hour)
	go func() {
		for {
			<-ticker.C
			_, err := db.Exec(context.Background(), "REFRESH MATERIALIZED VIEW valid_trips_today")
			if err != nil {
				log.Printf("Failed to refresh valid_trips_today: %v", err)
			} else {
				log.Println("Refreshed valid_trips_today")
			}
		}
	}()
}
