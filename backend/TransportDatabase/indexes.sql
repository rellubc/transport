CREATE INDEX IF NOT EXISTS idx_asd_parent_stop_time ON active_stop_departures (stop_parent_station, departure_time, start_date, end_date) INCLUDE (stop_id, trip_id, trip_headsign, service_id, stop_name, arrival_time, pickup_type, drop_off_type, monday, tuesday, wednesday, thursday, friday, saturday, sunday);
CREATE UNIQUE INDEX IF NOT EXISTS idx_asd_unique ON active_stop_departures (trip_id, stop_sequence);

CREATE INDEX IF NOT EXISTS idx_stop_times_stop_departure ON stop_times (stop_id, departure_time) INCLUDE (trip_id, arrival_time, pickup_type, drop_off_type, stop_headsign, stop_sequence);
CREATE INDEX IF NOT EXISTS idx_stop_times_stop_id ON stop_times (stop_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_trip_id ON stop_times (trip_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_trip_stop ON stop_times (trip_id, stop_id) INCLUDE (arrival_time, departure_time, pickup_type, drop_off_type);

CREATE INDEX IF NOT EXISTS idx_trips_route_id ON trips (route_id);
CREATE INDEX IF NOT EXISTS idx_trips_service_id ON trips (service_id) INCLUDE (trip_id, trip_headsign, route_id);

CREATE INDEX IF NOT EXISTS stops_stop_parent_station_idx ON stops (stop_parent_station);

CREATE INDEX IF NOT EXISTS idx_calendars_date_range ON calendars (start_date, end_date);

CREATE INDEX IF NOT EXISTS idx_consist_trip_timestamp ON consist (trip_id, timestamp DESC);

CREATE UNIQUE INDEX IF NOT EXISTS vehicle_boardings_unique ON vehicle_boardings (vehicle_category_id, child_sequence, grandchild_sequence, boarding_area_id);