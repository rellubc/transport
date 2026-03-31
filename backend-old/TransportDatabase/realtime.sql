-- NEED TO UPDATE SCHEMA

CREATE EXTENSION IF NOT EXISTS postgis;

CREATE TABLE IF NOT EXISTS trip_updates (
    trip_id TEXT NOT NULL,
    trip_route_id TEXT,
    trip_direction_id SMALLINT,
    trip_start_time TEXT,
    trip_start_date TEXT,
    trip_schedule_relationship SMALLINT,

    vehicle_id TEXT,
    vehicle_label TEXT,
    vehicle_license_plate TEXT,
    vehicle_air_conditioned BOOLEAN,
    vehicle_wheelchair_accessible SMALLINT,
    vehicle_model TEXT,
    vehicle_performing_prior_trip BOOLEAN,
    vehicle_special_vehicle_attributes INT,

    stu_stop_sequence INT,
    stu_stop_id TEXT,
    stu_arrival_delay INT,
    stu_arrival_time BIGINT,
    stu_arrival_uncertainty INT,
    stu_departure_delay INT,
    stu_departure_time BIGINT,
    stu_departure_uncertainty INT,
    stu_schedule_relationship SMALLINT,
    stu_departure_occupancy_status SMALLINT,
    stu_carriage_name TEXT,
    stu_carriage_position_in_consist INT NOT NULL,
    stu_carriage_occupancy_status SMALLINT,
    stu_carriage_quiet_carriage BOOLEAN,
    stu_carriage_toilet BOOLEAN,
    stu_carriage_luggage_rack BOOLEAN,
    stu_carriage_departure_occupancy_status SMALLINT,

    timestamp BIGINT,
    delay INT,

    PRIMARY KEY (trip_id, stu_stop_sequence, stu_carriage_position_in_consist)
);

CREATE TABLE IF NOT EXISTS vehicle_positions (
    trip_id TEXT,
    trip_route_id TEXT,
    trip_direction_id SMALLINT,
    trip_start_time TEXT,
    trip_start_date TEXT,
    trip_schedule_relationship SMALLINT,

    vehicle_id TEXT NOT NULL,
    vehicle_label TEXT,
    vehicle_license_plate TEXT,
    vehicle_air_conditioned BOOLEAN,
    vehicle_wheelchair_accessible BOOLEAN,
    vehicle_model TEXT,
    vehicle_performing_prior_trip BOOLEAN,
    vehicle_special_vehicle_attributes INT,

    position_latitude NUMERIC(9,6),
    position_longitude NUMERIC(9,6),
    geom GEOGRAPHY(POINT, 4326),
    position_bearing NUMERIC(9,6),
    position_odometer NUMERIC(9,2),
    position_speed NUMERIC(9,2),
    position_track_direction SMALLINT,

    current_stop_sequence INT,
    stop_id TEXT,
    current_status SMALLINT,
    timestamp BIGINT,
    congestion_level SMALLINT,
    occupancy_status SMALLINT,

    stu_carriage_name TEXT,
    stu_carriage_position_in_consist INT NOT NULL,
    stu_carriage_occupancy_status SMALLINT,
    stu_carriage_quiet_carriage BOOLEAN,
    stu_carriage_toilet BOOLEAN,
    stu_carriage_luggage_rack BOOLEAN,
    stu_carriage_departure_occupancy_status SMALLINT,

    PRIMARY KEY (vehicle_id, stu_carriage_position_in_consist)
);

CREATE INDEX idx_trip_updates_timestamp ON trip_updates(timestamp);

CREATE INDEX idx_vehicle_positions_timestamp ON vehicle_positions(timestamp);
CREATE INDEX idx_vehicle_positions_geom ON vehicle_positions USING GIST(geom);
