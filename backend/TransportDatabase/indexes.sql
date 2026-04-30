CREATE INDEX idx_asd_parent_stop_time ON public.active_stop_departures USING btree (stop_parent_station, departure_time, start_date, end_date) INCLUDE (stop_id, trip_id, trip_headsign, service_id, stop_name, arrival_time, pickup_type, drop_off_type, monday, tuesday, wednesday, thursday, friday, saturday, sunday);
CREATE UNIQUE INDEX idx_asd_unique ON public.active_stop_departures USING btree (trip_id, stop_sequence);

CREATE INDEX idx_stop_times_stop_departure ON public.stop_times USING btree (stop_id, departure_time) INCLUDE (trip_id, arrival_time, pickup_type, drop_off_type, stop_headsign, stop_sequence);
CREATE INDEX idx_stop_times_stop_id ON public.stop_times USING btree (stop_id);
CREATE INDEX idx_stop_times_trip_id ON public.stop_times USING btree (trip_id);
CREATE INDEX idx_stop_times_trip_stop ON public.stop_times USING btree (trip_id, stop_id) INCLUDE (arrival_time, departure_time, pickup_type, drop_off_type);

CREATE INDEX idx_trips_route_id ON public.trips USING btree (route_id);
CREATE INDEX idx_trips_service_id ON public.trips USING btree (service_id) INCLUDE (trip_id, trip_headsign, route_id);

CREATE INDEX stops_stop_parent_station_idx ON public.stops USING btree (stop_parent_station);