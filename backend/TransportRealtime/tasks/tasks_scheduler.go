package tasks

import (
	"context"
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
)

func StartScheduler(ctx context.Context, apiKey string, database *pgxpool.Pool) {
	realtimeTasks := RealtimeTasks()
	for i, realtimeTask := range realtimeTasks {
		go realtimeTask.StartRealtimeTask(ctx, apiKey, time.Duration(i+1)*time.Second, database)
	}
}
