package main

import (
	"TransportRealtime/config"
	"TransportRealtime/db"
	"TransportRealtime/handlers"
	"TransportRealtime/repositories"
	"context"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"
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

	repos := repositories.NewRepositories(database)
	router := handlers.RegisterRoutes(repos)

	port := os.Getenv("GO_PORT")
	if port == "" {
		port = "8080"
	}

	server := &http.Server{
		Addr:    ":" + port,
		Handler: router,
	}

	go func() {
		log.Printf("Server running on port %s", port)

		if err := server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatal(err)
		}
	}()

	<-ctx.Done()

	log.Println("Shutting down server...")

	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer shutdownCancel()

	if err := server.Shutdown(shutdownCtx); err != nil {
		log.Fatal(err)
	}

	log.Println("Server shut down successfully")
}

func handleShutdown(cancel context.CancelFunc) {
	c := make(chan os.Signal, 1)

	signal.Notify(c, os.Interrupt, syscall.SIGTERM)

	<-c
	cancel()
}
