package main

import (
	"TransportRealtime/config"
	"TransportRealtime/db"
	"TransportRealtime/tasks"
	"context"
	"log"
	"os"
	"os/signal"
	"syscall"
)

func main() {
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()
	go handleShutdown(cancel)

	cfg, err := config.Load()
	if err != nil {
		log.Fatal(err)
	}

	database, err := db.Connect(cfg.DBUrl)
	if err != nil {
		log.Fatal(err)
	}
	defer database.Close()

	tasks.StartScheduler(ctx, cfg.APIKey, database)

	<-ctx.Done()
}

func handleShutdown(cancel context.CancelFunc) {
	c := make(chan os.Signal, 1)

	signal.Notify(c, os.Interrupt, syscall.SIGTERM)

	<-c
	cancel()
}
