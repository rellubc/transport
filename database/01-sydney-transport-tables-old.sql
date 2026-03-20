CREATE TABLE agency (
    agency_id VARCHAR(100) PRIMARY KEY,
    agency_name VARCHAR(100) NOT NULL,
    agency_url VARCHAR(255) NOT NULL,
    agency_timezone VARCHAR(100) NOT NULL,
    agency_lang VARCHAR(100) NOT NULL,
    agency_phone VARCHAR(100) NOT NULL,
    agency_fare_url VARCHAR(255),
    agency_email VARCHAR(100)
);

CREATE TABLE calendar (
    service_id VARCHAR(100) PRIMARY KEY,
    -- 0: Service is not available for all Mondays in the date range. 1: Service is available for all Mondays in the date range. -- 
    monday TINYINT(1) NOT NULL,
    -- 0: Service is not available for all Tuesdays in the date range. 1: Service is available for all Tuesdays in the date range. -- 
    tuesday TINYINT(1) NOT NULL,
    -- 0: Service is not available for all Wednesdays in the date range. 1: Service is available for all Wednesdays in the date range. -- 
    wednesday TINYINT(1) NOT NULL,
    -- 0: Service is not available for all Thursdays in the date range. 1: Service is available for all Thursdays in the date range. -- 
    thursday TINYINT(1) NOT NULL,
    -- 0: Service is not available for all Fridays in the date range. 1: Service is available for all Fridays in the date range. -- 
    friday TINYINT(1) NOT NULL,
    -- 0: Service is not available for all Saturdays in the date range. 1: Service is available for all Saturdays in the date range. -- 
    saturday TINYINT(1) NOT NULL,
    -- 0: Service is not available for all Sundays in the date range. 1: Service is available for all Sundays in the date range. -- 
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
    shape_dist_travelled DECIMAL(18,2),
    PRIMARY KEY (shape_id, shape_pt_sequence)
);

CREATE TABLE stops (
    stop_id VARCHAR(100) PRIMARY KEY,
    stop_code VARCHAR(100),
    stop_name VARCHAR(100) NOT NULL,
    stop_desc VARCHAR(100),
    stop_lat DECIMAL(9,6) NOT NULL,
    stop_lon DECIMAL(9,6) NOT NULL,
    zone_id VARCHAR(100),
    stop_url VARCHAR(255),
    -- 0: Platform, 1: Station, 2: Entrance/Exit, 3: Generic Node (UNUSED), 4: Boarding Area (UNUSED) --
    location_type TINYINT(1) NOT NULL,
    parent_station VARCHAR(100),
    stop_timezone VARCHAR(100),
    -- https://gtfs.org/documentation/schedule/reference/#agencytxt --
    wheelchair_boarding TINYINT(1) NOT NULL,
    platform_code INT
);

CREATE TABLE routes (
    route_id VARCHAR(100) PRIMARY KEY,
    agency_id VARCHAR(100) NOT NULL,
    route_short_name VARCHAR(100) NOT NULL,
    route_long_name VARCHAR(100) NOT NULL,
    route_desc TEXT NOT NULL,
    -- 0: Light Rail, 401: Metro, 2: Rail, 3: Bus, 4: Ferry, 105: Regional Rail --
    route_type INT NOT NULL,
    route_colour VARCHAR(6) NOT NULL,
    route_text_colour VARCHAR(6) NOT NULL,
    route_url VARCHAR(100) NOT NULL,
    FOREIGN KEY (agency_id) REFERENCES agency(agency_id)
);

CREATE TABLE trips (
    route_id VARCHAR(100) NOT NULL,
    service_id VARCHAR(100) NOT NULL,
    trip_id VARCHAR(100) PRIMARY KEY,
    trip_headsign VARCHAR(100) NOT NULL,
    trip_short_name VARCHAR(100) NOT NULL,
    -- 0: Travel in one direction (e.g. outbound travel), 1: Travel in the opposite direction (e.g. inbound travel). --
    direction_id TINYINT(1) NOT NULL,
    block_id VARCHAR(100) NOT NULL,
    shape_id VARCHAR(100) NOT NULL,
    -- 0: No accessibility information for the trip. --
    -- 1: Vehicle being used on this particular trip can accommodate at least one rider in a wheelchair. --
    -- 2: No riders in wheelchairs can be accommodated on this trip. --
    wheelchair_accessible TINYINT(1) NOT NULL,
    route_direction TEXT,
    -- 0: No bike information for the trip. --
    -- 1: Vehicle being used on this particular trip can accommodate at least one bicycle. --
    -- 2: No bicycles are allowed on this trip. --
    bikes_allowed TINYINT(1),
    trip_note VARCHAR(100),
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
    stop_headsign VARCHAR(100) NOT NULL,
    -- 0: Regularly scheduled pickup. --
    -- 1: No pickup available. --
    -- 2: Must phone agency to arrange pickup. --
    -- 3: Must coordinate with driver to arrange pickup. --
    pickup_type TINYINT(1) NOT NULL,
    -- 0: Regularly scheduled drop off. --
    -- 1: No drop off available. --
    -- 2: Must phone agency to arrange drop off. --
    -- 3: Must coordinate with driver to arrange drop off. --
    drop_off_type TINYINT(1) NOT NULL,
    shape_dist_travelled DECIMAL(18,2) NOT NULL,
    -- 0: Approximate times, 1: Exact times. --
    timepoint TINYINT(1),
    stop_note VARCHAR(100),
    PRIMARY KEY (trip_id, stop_sequence),
    FOREIGN KEY (trip_id) REFERENCES trips(trip_id),
    FOREIGN KEY (stop_id) REFERENCES stops(stop_id)
);

CREATE TABLE vehicle_boardings (
    vehicle_category_id VARCHAR(100) NOT NULL,
    child_sequence INT NOT NULL,
    grandchild_sequence INT NOT NULL,
    boarding_area_id INT NOT NULL,
    PRIMARY KEY (vehicle_category_id, child_sequence, boarding_area_id),
    FOREIGN KEY (vehicle_category_id) REFERENCES vehicle_categories(vehicle_category_id)
);

CREATE TABLE vehicle_couplings (
    parent_id VARCHAR(100) NOT NULL,
    child_id VARCHAR(100) NOT NULL,
    child_sequence INT NOT NULL,
    child_label VARCHAR(100) NOT NULL,
    PRIMARY KEY (parent_id, child_id, child_sequence),
    FOREIGN KEY (parent_id) REFERENCES vehicle_categories(vehicle_category_id),
    FOREIGN KEY (child_id) REFERENCES vehicle_categories(vehicle_category_id)
);

CREATE TABLE occupancies (
    trip_id VARCHAR(100) NOT NULL,
    stop_sequence INT NOT NULL,
    -- 0: Empty, 1: Many Seats Available, 2: Few Seats Available, 3: Standing Room Only, 4: Crushed Standing Room Only, 5: Full, 6: Not Accepting Passengers --
    occupancy_status TINYINT(1) NOT NULL,
    -- 0: The occupancy level does not apply for all Mondays in the date range. 1: The occupancy level applies for all Mondays in the date range. --
    monday TINYINT(1) NOT NULL,
    -- 0: The occupancy level does not apply for all Tuesdays in the date range. 1: The occupancy level applies for all Tuesdays in the date range. --
    tuesday TINYINT(1) NOT NULL,
    -- 0: The occupancy level does not apply for all Wednesdays in the date range. 1: The occupancy level applies for all Wednesdays in the date range. --
    wednesday TINYINT(1) NOT NULL,
    -- 0: The occupancy level does not apply for all Thursdays in the date range. 1: The occupancy level applies for all Thursdays in the date range. --
    thursday TINYINT(1) NOT NULL,
    -- 0: The occupancy level does not apply for all Fridays in the date range. 1: The occupancy level applies for all Fridays in the date range. --
    friday TINYINT(1) NOT NULL,
    -- 0: The occupancy level does not apply for all Saturdays in the date range. 1: The occupancy level applies for all Saturdays in the date range. --
    saturday TINYINT(1) NOT NULL,
    -- 0: The occupancy level does not apply for all Sundays in the date range. 1: The occupancy level applies for all Sundays in the date range. --
    sunday TINYINT(1) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    -- 0: Does not ovveride another occupancy description. 1: Overrides another occupancy description --
    exception TINYINT(1),
    PRIMARY KEY (trip_id, stop_sequence, start_date),
    FOREIGN KEY (trip_id) REFERENCES trips(trip_id),
);
