package tasks

import (
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
)

func StartTasks(apiKey string, database *pgxpool.Pool, interval time.Duration) {
	tasks := GetRealtimeTasks()

	for i, task := range tasks {
		task.DB = database
		go task.Start(apiKey, time.Duration(i+1)*time.Second, interval)
	}
}
