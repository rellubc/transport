package db

import (
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"
)

func Connect(DBUrl string) (*pgxpool.Pool, error) {
	config, err := pgxpool.ParseConfig(DBUrl)
	if err != nil {
		return nil, fmt.Errorf("Unable to parse DB URL: %v", err)
	}

	pool, err := pgxpool.NewWithConfig(context.Background(), config)
	if err != nil {
		return nil, fmt.Errorf("Unable to connect to DB: %v", err)
	}

	return pool, err
}
