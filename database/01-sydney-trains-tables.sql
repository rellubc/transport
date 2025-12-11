CREATE TABLE agency (
    agency_id VARCHAR(255) PRIMARY KEY,
    agency_name VARCHAR(255) NOT NULL,
    agency_url VARCHAR(255) NOT NULL DEFAULT 'http://transportnsw.info',
    agency_timezone VARCHAR(255) NOT NULL DEFAULT 'Australia/Sydney',
    agency_lang VARCHAR(255) NOT NULL DEFAULT 'EN',
    agency_phone VARCHAR(255) NOT NULL DEFAULT '131500'
);

CREATE TABLE calendar (
    service_id VARCHAR(255) PRIMARY KEY,
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

CREATE TABLE routes (
    route_id VARCHAR(255) PRIMARY KEY,
    agency_id VARCHAR(255) NOT NULL,
    route_short_name VARCHAR(255) NOT NULL,
    route_long_name VARCHAR(255) NOT NULL,
    route_desc VARCHAR(255) NOT NULL,
    route_type INT NOT NULL,
    route_url VARCHAR(255) NOT NULL,
    route_color VARCHAR(6) NOT NULL DEFAULT '00B5EF',
    route_text_color VARCHAR(6) NOT NULL DEFAULT 'FFFFFF',
    FOREIGN KEY (agency_id) REFERENCES agency(agency_id)
);

CREATE TABLE shapes (
    shape_id VARCHAR(255) NOT NULL,
    shape_pt_lat DECIMAL(11,8) NOT NULL,
    shape_pt_lon DECIMAL(11,8) NOT NULL,
    shape_pt_sequence INT NOT NULL,
    shape_dist_traveled DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (shape_id, shape_pt_sequence)
);

CREATE TABLE trips (
    route_id VARCHAR(255) NOT NULL,
    service_id VARCHAR(255) NOT NULL,
    trip_id VARCHAR(255) PRIMARY KEY,
    shape_id VARCHAR(255) NOT NULL,
    trip_headsign VARCHAR(255) NOT NULL,
    trip_short_name VARCHAR(255) NOT NULL,
    direction_id TINYINT(1) NOT NULL,
    block_id VARCHAR(255) NOT NULL,
    wheelchair_accessible TINYINT(1) NOT NULL,
    vehicle_category_id VARCHAR(255) NOT NULL,
    FOREIGN KEY (route_id) REFERENCES routes(route_id),
    FOREIGN KEY (service_id) REFERENCES calendar(service_id)
);

CREATE TABLE stops (
    stop_id VARCHAR(255) PRIMARY KEY,
    stop_code VARCHAR(255) NOT NULL,
    stop_name VARCHAR(255) NOT NULL,
    stop_desc VARCHAR(255) NOT NULL,
    stop_lat DECIMAL(11,8) NOT NULL,
    stop_lon DECIMAL(11,8) NOT NULL,
    zone_id VARCHAR(255) NOT NULL,
    stop_url VARCHAR(255) NOT NULL,
    location_type VARCHAR(255) NOT NULL,
    parent_station VARCHAR(255) NOT NULL,
    stop_timezone VARCHAR(255) NOT NULL,
    wheelchair_boarding VARCHAR(1) NOT NULL
);

CREATE TABLE stop_times (
    trip_id VARCHAR(255) NOT NULL,
    arrival_time TIME NOT NULL,
    departure_time TIME NOT NULL,
    stop_id VARCHAR(255) NOT NULL,
    stop_sequence INT NOT NULL,
    stop_headsign VARCHAR(255),
    pickup_type TINYINT(1) NOT NULL,
    drop_off_type TINYINT(1) NOT NULL,
    shape_dist_traveled DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (trip_id, stop_sequence),
    FOREIGN KEY (trip_id) REFERENCES trips(trip_id),
    FOREIGN KEY (stop_id) REFERENCES stops(stop_id)
);

CREATE TABLE vehicle_categories (
    vehicle_category_id VARCHAR(255) PRIMARY KEY,
    vehicle_category_name VARCHAR(255) NOT NULL
);

CREATE TABLE vehicle_boardings (
    vehicle_category_id VARCHAR(100) NOT NULL,
    child_sequence VARCHAR(100) NOT NULL,
    grandchild_sequence VARCHAR(100) NOT NULL,
    boarding_area_id VARCHAR(100) NOT NULL,
    PRIMARY KEY (vehicle_category_id, child_sequence, grandchild_sequence, boarding_area_id),
    FOREIGN KEY (vehicle_category_id) REFERENCES vehicle_categories(vehicle_category_id)
);

CREATE TABLE vehicle_couplings (
    parent_id VARCHAR(100) NOT NULL,
    child_id VARCHAR(100) NOT NULL,
    child_sequence VARCHAR(100) NOT NULL,
    child_label VARCHAR(100) NOT NULL,
    PRIMARY KEY (parent_id, child_id, child_sequence),
    FOREIGN KEY (parent_id) REFERENCES vehicle_categories(vehicle_category_id),
    FOREIGN KEY (child_id) REFERENCES vehicle_categories(vehicle_category_id)
);