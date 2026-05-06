CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS pg_prewarm;

CREATE TABLE IF NOT EXISTS agencies (
    agency_id TEXT NOT NULL,
    agency_name TEXT NOT NULL,
    agency_url TEXT NOT NULL,
    agency_timezone TEXT NOT NULL,
    agency_language TEXT NOT NULL,
    agency_phone TEXT,
    agency_fare_url TEXT,
    agency_email TEXT,

    PRIMARY KEY (agency_id)
);

CREATE TABLE IF NOT EXISTS calendars (
    service_id TEXT NOT NULL,
    monday BOOLEAN NOT NULL,
    tuesday BOOLEAN NOT NULL,
    wednesday BOOLEAN NOT NULL,
    thursday BOOLEAN NOT NULL,
    friday BOOLEAN NOT NULL,
    saturday BOOLEAN NOT NULL,
    sunday BOOLEAN NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,

    CHECK (start_date <= end_date),

    PRIMARY KEY (service_id)
);

CREATE TABLE IF NOT EXISTS notes (
    note_id TEXT NOT NULL,
    note_text TEXT NOT NULL,

    PRIMARY KEY (note_id)
);

CREATE TABLE IF NOT EXISTS vehicle_categories (
    vehicle_category_id TEXT NOT NULL,
    vehicle_category_name TEXT NOT NULL,

    PRIMARY KEY (vehicle_category_id)
);

CREATE TABLE IF NOT EXISTS routes (
    route_id TEXT NOT NULL,
    agency_id TEXT NOT NULL,
    route_short_name TEXT,
    route_long_name TEXT NOT NULL,
    route_desc TEXT NOT NULL,
    route_type INT NOT NULL,
    route_colour TEXT NOT NULL,
    route_text_colour TEXT NOT NULL,
    route_url TEXT,

    PRIMARY KEY (route_id),

    FOREIGN KEY (agency_id) REFERENCES agencies(agency_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS shapes (
    shape_id TEXT NOT NULL,
    shape_pt_lat NUMERIC(9,6) NOT NULL,
    shape_pt_lon NUMERIC(9,6) NOT NULL,
    shape_geom GEOGRAPHY(POINT, 4326) NOT NULL,
    shape_pt_sequence INT NOT NULL,
    shape_dist_travelled NUMERIC(9,2),
    route_type INT,

    PRIMARY KEY (shape_id, shape_pt_sequence)
);

CREATE TABLE IF NOT EXISTS stops (
    stop_id TEXT NOT NULL,
    stop_code TEXT,
    stop_name TEXT NOT NULL,
    stop_desc TEXT,
    stop_lat NUMERIC(9,6) NOT NULL,
    stop_lon NUMERIC(9,6) NOT NULL,
    stop_geom GEOGRAPHY(POINT, 4326) NOT NULL,
    stop_zone_id TEXT,
    stop_url TEXT,
    stop_location_type SMALLINT NOT NULL,
    stop_parent_station TEXT,
    stop_timezone TEXT,
    stop_wheelchair_boarding SMALLINT NOT NULL,
    stop_platform_code TEXT,
    route_type INT,

    PRIMARY KEY (stop_id, route_type)
);

CREATE TABLE IF NOT EXISTS trips (
    route_id TEXT NOT NULL,
    service_id TEXT NOT NULL,
    trip_id TEXT NOT NULL,
    trip_headsign TEXT,
    trip_short_name TEXT,
    trip_direction_id SMALLINT NOT NULL,
    trip_block_id TEXT,
    shape_id TEXT,
    trip_wheelchair_accessible SMALLINT NOT NULL,
    trip_route_direction TEXT,
    trip_bikes_allowed SMALLINT,
    trip_note TEXT,
    vehicle_category_id TEXT,

    PRIMARY KEY (trip_id),

    FOREIGN KEY (route_id) REFERENCES routes(route_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS stop_times (
    trip_id TEXT NOT NULL,
    arrival_time INT NOT NULL,
    departure_time INT NOT NULL,
    stop_id TEXT NOT NULL,
    stop_sequence INT NOT NULL,
    stop_headsign TEXT,
    pickup_type SMALLINT NOT NULL,
    drop_off_type SMALLINT NOT NULL,
    shape_dist_travelled NUMERIC(9,2),
    timepoint INT,
    stop_note TEXT,
    route_type INT NOT NULL,

    PRIMARY KEY (trip_id, stop_sequence),

    FOREIGN KEY (trip_id) REFERENCES trips(trip_id) ON DELETE CASCADE,
    FOREIGN KEY (stop_id, route_type) REFERENCES stops(stop_id, route_type) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS vehicle_boardings (
    vehicle_category_id TEXT NOT NULL,
    child_sequence INT NOT NULL,
    grandchild_sequence INT,
    boarding_area_id TEXT NOT NULL,

    PRIMARY KEY (vehicle_category_id, child_sequence, boarding_area_id),

    FOREIGN KEY (vehicle_category_id) REFERENCES vehicle_categories(vehicle_category_id)
);

CREATE TABLE IF NOT EXISTS vehicle_couplings (
    parent_id TEXT NOT NULL,
    child_id TEXT NOT NULL,
    child_sequence INT NOT NULL,
    child_label TEXT NOT NULL,

    PRIMARY KEY (parent_id, child_id, child_sequence),

    FOREIGN KEY (parent_id) REFERENCES vehicle_categories(vehicle_category_id),
    FOREIGN KEY (child_id) REFERENCES vehicle_categories(vehicle_category_id)
);
