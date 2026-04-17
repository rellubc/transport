CREATE EXTENSION IF NOT EXISTS postgis;

CREATE TABLE IF NOT EXISTS trip_updates (
    trip_id TEXT NOT NULL,
    trip_route_id TEXT NOT NULL,
    trip_schedule_relationship TEXT NOT NULL,

    vehicle_id TEXT,
    vehicle_label TEXT,
    vehicle_model TEXT NOT NULL,

    timestamp TIMESTAMPTZ NOT NULL,

    PRIMARY KEY (trip_id)
);

CREATE TABLE IF NOT EXISTS stop_time_updates (
    trip_id TEXT NOT NULL,

    stop_id TEXT NOT NULL,
    stop_arrival_time TIMESTAMPTZ,
    stop_arrival_delay BIGINT,
    stop_departure_time TIMESTAMPTZ,
    stop_departure_delay BIGINT,
    timestamp TIMESTAMPTZ,

    PRIMARY KEY (trip_id, stop_id),
    FOREIGN KEY (trip_id) REFERENCES trip_updates(trip_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS carriage_occupancies (
    trip_id TEXT NOT NULL,
    stop_id TEXT NOT NULL,

    position_in_consist INT NOT NULL,
    departure_occupancy_status TEXT NOT NULL,

    PRIMARY KEY (trip_id, stop_id, position_in_consist),
    FOREIGN KEY (trip_id) REFERENCES trip_updates(trip_id) ON DELETE CASCADE,
    FOREIGN KEY (trip_id, stop_id) REFERENCES stop_time_updates(trip_id, stop_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS vehicle_positions (
    trip_id TEXT NOT NULL,
    trip_route_id TEXT NOT NULL,
    trip_schedule_relationship TEXT NOT NULL,

    vehicle_id TEXT NOT NULL,
    vehicle_label TEXT NOT NULL,
    vehicle_model TEXT NOT NULL,

    position_latitude NUMERIC(9,6) NOT NULL,
    position_longitude NUMERIC(9,6) NOT NULL,
    position_geom GEOGRAPHY(POINT, 4326) NOT NULL,

    stop_id TEXT NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL,
    congestion_level TEXT NOT NULL,
    occupancy_status TEXT,

    PRIMARY KEY (vehicle_id)
);

CREATE TABLE IF NOT EXISTS consist (
    vehicle_id TEXT NOT NULL,

    position_in_consist INT NOT NULL,
    occupancy_status TEXT,
    timestamp TIMESTAMPTZ NOT NULL,

    PRIMARY KEY (vehicle_id, position_in_consist, timestamp),
    FOREIGN KEY (vehicle_id) REFERENCES vehicle_positions(vehicle_id) ON DELETE CASCADE
);

CREATE INDEX idx_vehicle_positions_geom ON vehicle_positions USING GIST(position_geom);
