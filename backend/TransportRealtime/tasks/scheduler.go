package tasks

import "time"

func StartTasks(apiKey string, interval time.Duration) {
	tasks := GetRealtimeTasks()

	for i, task := range tasks {
		go task.Start(apiKey, time.Duration(i+1)*time.Second, interval)
	}
}
