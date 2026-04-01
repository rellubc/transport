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

	agencyRepo := &repositories.AgencyRepository{DB: database}

	http.HandleFunc("/api/agencies", handlers.GetAgenciesHandler(agencyRepo))
	http.HandleFunc("/api/agency", handlers.GetAgencyHandler(agencyRepo))

	port := os.Getenv("GO_PORT")
	if port == "" {
		port = "8080"
	}
	err = http.ListenAndServe(":"+port, nil)
	if err != nil {
		log.Fatal(err)
	}
	log.Printf("Server running on port %s", port)

	tasks.StartTasks(apiKey, 15*time.Second)
	log.Println("Starting realtime data fetcher...")
	select {}
}
