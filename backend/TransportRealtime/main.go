package main

import (
	"log"
	"net/http"
	"os"
	"time"

	"TransportRealtime/db"
	"TransportRealtime/handlers"
	"TransportRealtime/repositories"
	"TransportRealtime/tasks"

	"github.com/joho/godotenv"
)

func main() {
	err := godotenv.Load("../.env")
	if err != nil {
		log.Fatalf("Error loading .env file: %v", err)
	}
	apiKey := os.Getenv("API_KEY")

	database := db.Connect()
	defer database.Close()

	repos := repositories.NewRepositories(database)
	router := handlers.RegisterRoutes(repos)

	if err := repos.StopTime.WarmCache(); err != nil {
		log.Printf("warn: cache warmup failed: %v", err)
	}

	tasks.StartTasks(apiKey, database, 30*time.Second)
	log.Println("Starting realtime data fetcher...")

	port := os.Getenv("GO_PORT")
	if port == "" {
		port = "8080"
	}

	log.Printf("Server running on port %s", port)
	err = http.ListenAndServe(":"+port, router)
	if err != nil {
		log.Fatal(err)
	}
}
