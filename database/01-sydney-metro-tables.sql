CREATE TABLE agency (
    agency_id VARCHAR(100) PRIMARY KEY,
    agency_name VARCHAR(100) NOT NULL,
    agency_url VARCHAR(100) NOT NULL DEFAULT 'http://transportnsw.info',
    agency_timezone VARCHAR(100) NOT NULL DEFAULT 'Australia/Sydney',
    agency_lang VARCHAR(100) NOT NULL DEFAULT 'EN',
    agency_phone VARCHAR(100) NOT NULL DEFAULT '131500',
    agency_fare_url VARCHAR(100) DEFAULT 'http://transportnsw.info',
    agency_email VARCHAR(100) DEFAULT 'information@transport.nsw.gov.au'
);

CREATE TABLE calendar (
    service_id VARCHAR(100) PRIMARY KEY,
    monday TINYINT(1) NOT NULL,
    tuesday TINYINT(1) NOT NULL,
    wednesday TINYINT(1) NOT NULL,
    thursday TINYINT(1) NOT NULL,
    friday TINYINT(1) NOT NULL,
    saturday TINYINT(1) NOT NULL,
    sunday TINYINT(1) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL
);

CREATE TABLE notes (
    note_id VARCHAR(100) PRIMARY KEY,
    note_text TEXT NOT NULL
);

CREATE TABLE vehicle_categories (
    vehicle_category_id VARCHAR(100) PRIMARY KEY,
    vehicle_category_name VARCHAR(100) NOT NULL
);

CREATE TABLE shapes (
    shape_id VARCHAR(100) NOT NULL,
    shape_pt_lat DECIMAL(9,6) NOT NULL,
    shape_pt_lon DECIMAL(9,6) NOT NULL,
    shape_pt_sequence INT NOT NULL,
    shape_dist_travelled DECIMAL(18,2) NOT NULL,
    mode VARCHAR(100) NOT NULL,
    PRIMARY KEY (shape_id, shape_pt_sequence, mode)
);

CREATE TABLE stops (
    stop_id VARCHAR(100) NOT NULL,
    stop_code VARCHAR(100),
    stop_name VARCHAR(100) NOT NULL,
    stop_desc VARCHAR(100),
    stop_lat DECIMAL(9,6) NOT NULL,
    stop_lon DECIMAL(9,6) NOT NULL,
    zone_id VARCHAR(100),
    stop_url VARCHAR(100),
    location_type TINYINT(1) NOT NULL,
    parent_station VARCHAR(100),
    stop_timezone VARCHAR(100),
    wheelchair_boarding TINYINT(1) NOT NULL,
    platform_code INT,
    mode VARCHAR(100) NOT NULL,
    network VARCHAR(100),
    PRIMARY KEY (stop_id, mode)
);

CREATE TABLE routes (
    route_id VARCHAR(100) PRIMARY KEY,
    agency_id VARCHAR(100) NOT NULL,
    route_short_name VARCHAR(100) NOT NULL,
    route_long_name VARCHAR(100) NOT NULL,
    route_desc VARCHAR(100) NOT NULL,
    route_type INT NOT NULL,
    route_colour VARCHAR(6) NOT NULL DEFAULT '00B5EF',
    route_text_colour VARCHAR(6) NOT NULL DEFAULT 'FFFFFF',
    route_url VARCHAR(100) NOT NULL,
    FOREIGN KEY (agency_id) REFERENCES agency(agency_id)
);

CREATE TABLE trips (
    route_id VARCHAR(100) NOT NULL,
    service_id VARCHAR(100) NOT NULL,
    trip_id VARCHAR(100) PRIMARY KEY,
    shape_id VARCHAR(100) NOT NULL,
    trip_headsign VARCHAR(100) NOT NULL,
    direction_id TINYINT(1) NOT NULL,
    trip_short_name VARCHAR(100) NOT NULL,
    block_id VARCHAR(100) NOT NULL,
    wheelchair_accessible TINYINT(1) NOT NULL,
    trip_note VARCHAR(100),
    route_direction TEXT,
    bikes_allowed TINYINT(1),
    vehicle_category_id VARCHAR(100),
    FOREIGN KEY (route_id) REFERENCES routes(route_id),
    FOREIGN KEY (service_id) REFERENCES calendar(service_id)
);

CREATE TABLE stop_times (
    trip_id VARCHAR(100) NOT NULL,
    arrival_time TIME NOT NULL,
    departure_time TIME NOT NULL,
    stop_id VARCHAR(100) NOT NULL,
    stop_sequence INT NOT NULL,
    stop_headsign VARCHAR(100),
    pickup_type TINYINT(1) NOT NULL,
    drop_off_type TINYINT(1) NOT NULL,
    shape_dist_travelled DECIMAL(18,2) NOT NULL,
    timepoint TINYINT(1),
    stop_note VARCHAR(100),
    mode VARCHAR(100) NOT NULL,
    PRIMARY KEY (trip_id, stop_sequence),
    FOREIGN KEY (trip_id) REFERENCES trips(trip_id),
    FOREIGN KEY (stop_id, mode) REFERENCES stops(stop_id, mode)
);

CREATE TABLE vehicle_boardings (
    vehicle_category_id VARCHAR(100) NOT NULL,
    child_sequence INT NOT NULL,
    grandchild_sequence INT,
    boarding_area_id INT NOT NULL,
    PRIMARY KEY (vehicle_category_id, child_sequence, boarding_area_id),
    FOREIGN KEY (vehicle_category_id) REFERENCES vehicle_categories(vehicle_category_id)
);

CREATE TABLE vehicle_couplings (
    parent_id VARCHAR(100) NOT NULL,
    child_id VARCHAR(100) NOT NULL,
    child_sequence INT NOT NULL,
    child_label INT NOT NULL,
    PRIMARY KEY (parent_id, child_id, child_sequence),
    FOREIGN KEY (parent_id) REFERENCES vehicle_categories(vehicle_category_id),
    FOREIGN KEY (child_id) REFERENCES vehicle_categories(vehicle_category_id)
);
