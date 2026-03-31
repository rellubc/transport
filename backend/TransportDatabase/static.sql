CREATE EXTENSION IF NOT EXISTS postgis;

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
    -- 0: Service is not available for all Mondays in the date range. 1: Service is available for all Mondays in the date range. -- 
    monday BOOLEAN NOT NULL,
    -- 0: Service is not available for all Tuesdays in the date range. 1: Service is available for all Tuesdays in the date range. -- 
    tuesday BOOLEAN NOT NULL,
    -- 0: Service is not available for all Wednesdays in the date range. 1: Service is available for all Wednesdays in the date range. -- 
    wednesday BOOLEAN NOT NULL,
    -- 0: Service is not available for all Thursdays in the date range. 1: Service is available for all Thursdays in the date range. -- 
    thursday BOOLEAN NOT NULL,
    -- 0: Service is not available for all Fridays in the date range. 1: Service is available for all Fridays in the date range. -- 
    friday BOOLEAN NOT NULL,
    -- 0: Service is not available for all Saturdays in the date range. 1: Service is available for all Saturdays in the date range. -- 
    saturday BOOLEAN NOT NULL,
    -- 0: Service is not available for all Sundays in the date range. 1: Service is available for all Sundays in the date range. -- 
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

CREATE TABLE IF NOT EXISTS shapes (
    shape_id TEXT NOT NULL,
    shape_pt_lat NUMERIC(9,6) NOT NULL,
    shape_pt_lon NUMERIC(9,6) NOT NULL,
    geom GEOGRAPHY(POINT, 4326) NOT NULL,
    shape_pt_sequence INT NOT NULL,
    shape_dist_travelled NUMERIC(9,2),
    mode TEXT NOT NULL,

    PRIMARY KEY (shape_id, shape_pt_sequence)
);

CREATE TABLE IF NOT EXISTS stops (
    stop_id TEXT NOT NULL,
    stop_code TEXT,
    stop_name TEXT NOT NULL,
    stop_desc TEXT,
    stop_lat NUMERIC(9,6) NOT NULL,
    stop_lon NUMERIC(9,6) NOT NULL,
    geom GEOGRAPHY(POINT, 4326) NOT NULL,
    zone_id TEXT,
    stop_url TEXT,
    -- 0: Platform, 1: Station, 2: Entrance/Exit, 3: Generic Node (UNUSED), 4: Boarding Area (UNUSED) --
    location_type SMALLINT NOT NULL,
    parent_station TEXT,
    stop_timezone TEXT,
    -- https://gtfs.org/documentation/schedule/reference/#agencytxt --
    wheelchair_boarding SMALLINT NOT NULL,
    platform_code TEXT,
    mode TEXT NOT NULL,

    PRIMARY KEY (stop_id)
);

CREATE TABLE IF NOT EXISTS routes (
    route_id TEXT NOT NULL,
    agency_id TEXT NOT NULL,
    route_short_name TEXT,
    route_long_name TEXT NOT NULL,
    route_desc TEXT NOT NULL,
    -- 0: Light Rail, 401: Metro, 2: Rail, 3: Bus, 4: Ferry, 105: Regional Rail --
    route_type INT NOT NULL,
    route_colour TEXT NOT NULL,
    route_text_colour TEXT NOT NULL,
    route_url TEXT,

    PRIMARY KEY (route_id),

    FOREIGN KEY (agency_id) REFERENCES agencies(agency_id)
);

CREATE TABLE IF NOT EXISTS trips (
    route_id TEXT NOT NULL,
    service_id TEXT NOT NULL,
    trip_id TEXT NOT NULL,
    trip_headsign TEXT,
    trip_short_name TEXT,
    -- 0: Travel in one direction (e.g. outbound travel), 1: Travel in the opposite direction (e.g. inbound travel). --
    direction_id SMALLINT NOT NULL,
    block_id TEXT,
    shape_id TEXT NOT NULL,
    -- 0: No accessibility information for the trip. --
    -- 1: Vehicle being used on this particular trip can accommodate at least one rider in a wheelchair. --
    -- 2: No riders in wheelchairs can be accommodated on this trip. --
    wheelchair_accessible SMALLINT NOT NULL,
    route_direction TEXT,
    -- 0: No bike information for the trip. --
    -- 1: Vehicle being used on this particular trip can accommodate at least one bicycle. --
    -- 2: No bicycles are allowed on this trip. --
    bikes_allowed SMALLINT,
    trip_note TEXT,
    vehicle_category_id TEXT,

    PRIMARY KEY (trip_id),

    FOREIGN KEY (route_id) REFERENCES routes(route_id),
    FOREIGN KEY (service_id) REFERENCES calendars(service_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS stop_times (
    trip_id TEXT NOT NULL,
    arrival_time INT NOT NULL,
    departure_time INT NOT NULL,
    stop_id TEXT NOT NULL,
    stop_sequence INT NOT NULL,
    stop_headsign TEXT,
    -- 0: Regularly scheduled pickup. --
    -- 1: No pickup available. --
    -- 2: Must phone agency to arrange pickup. --
    -- 3: Must coordinate with driver to arrange pickup. --
    pickup_type SMALLINT NOT NULL,
    -- 0: Regularly scheduled drop off. --
    -- 1: No drop off available. --
    -- 2: Must phone agency to arrange drop off. --
    -- 3: Must coordinate with driver to arrange drop off. --
    drop_off_type SMALLINT NOT NULL,
    shape_dist_travelled NUMERIC(9,2),
    -- 0: Approximate times, 1: Exact times. --
    timepoint BOOLEAN,
    stop_note TEXT,
    mode TEXT NOT NULL,

    PRIMARY KEY (trip_id, stop_sequence),

    FOREIGN KEY (trip_id) REFERENCES trips(trip_id) ON DELETE CASCADE,
    FOREIGN KEY (stop_id) REFERENCES stops(stop_id)
);

CREATE TABLE IF NOT EXISTS vehicle_boardings (
    vehicle_category_id TEXT NOT NULL,
    child_sequence INT NOT NULL,
    grandchild_sequence INT,
    boarding_area_id INT NOT NULL,

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

CREATE TABLE IF NOT EXISTS occupancies (
    trip_id TEXT NOT NULL,
    stop_sequence INT NOT NULL,
    -- 0: Empty, 1: Many Seats Available, 2: Few Seats Available, 3: Standing Room Only, 4: Crushed Standing Room Only, 5: Full, 6: Not Accepting Passengers --
    occupancy_status SMALLINT NOT NULL,
    -- 0: The occupancy level does not apply for all Mondays in the date range. 1: The occupancy level applies for all Mondays in the date range. --
    monday BOOLEAN NOT NULL,
    -- 0: The occupancy level does not apply for all Tuesdays in the date range. 1: The occupancy level applies for all Tuesdays in the date range. --
    tuesday BOOLEAN NOT NULL,
    -- 0: The occupancy level does not apply for all Wednesdays in the date range. 1: The occupancy level applies for all Wednesdays in the date range. --
    wednesday BOOLEAN NOT NULL,
    -- 0: The occupancy level does not apply for all Thursdays in the date range. 1: The occupancy level applies for all Thursdays in the date range. --
    thursday BOOLEAN NOT NULL,
    -- 0: The occupancy level does not apply for all Fridays in the date range. 1: The occupancy level applies for all Fridays in the date range. --
    friday BOOLEAN NOT NULL,
    -- 0: The occupancy level does not apply for all Saturdays in the date range. 1: The occupancy level applies for all Saturdays in the date range. --
    saturday BOOLEAN NOT NULL,
    -- 0: The occupancy level does not apply for all Sundays in the date range. 1: The occupancy level applies for all Sundays in the date range. --
    sunday BOOLEAN NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    -- 0: Does not ovveride another occupancy description. 1: Overrides another occupancy description --
    exception BOOLEAN,

    PRIMARY KEY (trip_id, stop_sequence, start_date),

    FOREIGN KEY (trip_id) REFERENCES trips(trip_id),
    FOREIGN KEY (trip_id, stop_sequence) REFERENCES stop_times(trip_id, stop_sequence)
);

CREATE INDEX idx_calendar_start_date ON calendars(start_date);
CREATE INDEX idx_calendar_end_date ON calendars(end_date);

CREATE INDEX idx_shapes_geom ON shapes USING GIST(geom);

CREATE INDEX idx_stops_stop_name ON stops(stop_name);
CREATE INDEX idx_stops_mode ON stops(mode);
CREATE INDEX idx_stops_geom ON stops USING GIST(geom);

CREATE INDEX idx_trips_route_id ON trips(route_id);
CREATE INDEX idx_trips_service_id ON trips(service_id);

CREATE INDEX idx_stop_times_trip_stop ON stop_times(trip_id, stop_id);
