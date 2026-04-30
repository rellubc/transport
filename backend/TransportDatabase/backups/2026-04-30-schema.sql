--
-- PostgreSQL database dump
--

\restrict k8wcMpRG3qnRYkEaRJ0BVNN8ugmu35yVbUSRui5O3WA8525XxLjn6jN3SvK0lnI

-- Dumped from database version 18.3 (Debian 18.3-1.pgdg13+1)
-- Dumped by pg_dump version 18.3 (Debian 18.3-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: tiger; Type: SCHEMA; Schema: -; Owner: user
--

CREATE SCHEMA tiger;


ALTER SCHEMA tiger OWNER TO "user";

--
-- Name: topology; Type: SCHEMA; Schema: -; Owner: user
--

CREATE SCHEMA topology;


ALTER SCHEMA topology OWNER TO "user";

--
-- Name: SCHEMA topology; Type: COMMENT; Schema: -; Owner: user
--

COMMENT ON SCHEMA topology IS 'PostGIS Topology schema';


--
-- Name: fuzzystrmatch; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS fuzzystrmatch WITH SCHEMA public;


--
-- Name: EXTENSION fuzzystrmatch; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION fuzzystrmatch IS 'determine similarities and distance between strings';


--
-- Name: pg_prewarm; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pg_prewarm WITH SCHEMA public;


--
-- Name: EXTENSION pg_prewarm; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pg_prewarm IS 'prewarm relation data';


--
-- Name: postgis; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA public;


--
-- Name: EXTENSION postgis; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis IS 'PostGIS geometry and geography spatial types and functions';


--
-- Name: postgis_tiger_geocoder; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS postgis_tiger_geocoder WITH SCHEMA tiger;


--
-- Name: EXTENSION postgis_tiger_geocoder; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis_tiger_geocoder IS 'PostGIS tiger geocoder and reverse geocoder';


--
-- Name: postgis_topology; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS postgis_topology WITH SCHEMA topology;


--
-- Name: EXTENSION postgis_topology; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis_topology IS 'PostGIS topology spatial types and functions';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: calendars; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.calendars (
    service_id text NOT NULL,
    monday boolean NOT NULL,
    tuesday boolean NOT NULL,
    wednesday boolean NOT NULL,
    thursday boolean NOT NULL,
    friday boolean NOT NULL,
    saturday boolean NOT NULL,
    sunday boolean NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    CONSTRAINT calendars_check CHECK ((start_date <= end_date))
);


ALTER TABLE public.calendars OWNER TO "user";

--
-- Name: stop_times; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.stop_times (
    trip_id text NOT NULL,
    arrival_time integer NOT NULL,
    departure_time integer NOT NULL,
    stop_id text NOT NULL,
    stop_sequence integer NOT NULL,
    stop_headsign text,
    pickup_type smallint NOT NULL,
    drop_off_type smallint NOT NULL,
    shape_dist_travelled numeric(9,2),
    timepoint integer,
    stop_note text,
    route_type integer
);


ALTER TABLE public.stop_times OWNER TO "user";

--
-- Name: stops; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.stops (
    stop_id text NOT NULL,
    stop_code text,
    stop_name text NOT NULL,
    stop_desc text,
    stop_lat numeric(9,6) NOT NULL,
    stop_lon numeric(9,6) NOT NULL,
    stop_geom public.geography(Point,4326) NOT NULL,
    stop_zone_id text,
    stop_url text,
    stop_location_type smallint NOT NULL,
    stop_parent_station text,
    stop_timezone text,
    stop_wheelchair_boarding smallint NOT NULL,
    stop_platform_code text,
    route_type integer
);


ALTER TABLE public.stops OWNER TO "user";

--
-- Name: trips; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.trips (
    route_id text NOT NULL,
    service_id text NOT NULL,
    trip_id text NOT NULL,
    trip_headsign text,
    trip_short_name text,
    trip_direction_id smallint NOT NULL,
    trip_block_id text,
    shape_id text,
    trip_wheelchair_accessible smallint NOT NULL,
    trip_route_direction text,
    trip_bikes_allowed smallint,
    trip_note text,
    vehicle_category_id text
);


ALTER TABLE public.trips OWNER TO "user";

--
-- Name: active_stop_departures; Type: MATERIALIZED VIEW; Schema: public; Owner: user
--

CREATE MATERIALIZED VIEW public.active_stop_departures AS
 SELECT t.trip_id,
    t.trip_headsign,
    t.service_id,
    c.monday,
    c.tuesday,
    c.wednesday,
    c.thursday,
    c.friday,
    c.saturday,
    c.sunday,
    c.start_date,
    c.end_date,
    st.stop_id,
    s.stop_name,
    COALESCE(s.stop_parent_station, s.stop_id) AS stop_parent_station,
    st.arrival_time,
    st.departure_time,
    st.pickup_type,
    st.drop_off_type,
    st.stop_sequence
   FROM (((public.trips t
     JOIN public.calendars c ON ((c.service_id = t.service_id)))
     JOIN public.stop_times st ON ((st.trip_id = t.trip_id)))
     JOIN public.stops s ON ((s.stop_id = st.stop_id)))
  WITH NO DATA;


ALTER MATERIALIZED VIEW public.active_stop_departures OWNER TO "user";

--
-- Name: agencies; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.agencies (
    agency_id text NOT NULL,
    agency_name text NOT NULL,
    agency_url text NOT NULL,
    agency_timezone text NOT NULL,
    agency_language text NOT NULL,
    agency_phone text,
    agency_fare_url text,
    agency_email text
);


ALTER TABLE public.agencies OWNER TO "user";

--
-- Name: consist; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.consist (
    vehicle_id text NOT NULL,
    position_in_consist integer NOT NULL,
    occupancy_status text,
    "timestamp" timestamp with time zone NOT NULL,
    trip_id text NOT NULL
);


ALTER TABLE public.consist OWNER TO "user";

--
-- Name: notes; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.notes (
    note_id text NOT NULL,
    note_text text NOT NULL
);


ALTER TABLE public.notes OWNER TO "user";

--
-- Name: routes; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.routes (
    route_id text NOT NULL,
    agency_id text NOT NULL,
    route_short_name text,
    route_long_name text NOT NULL,
    route_desc text NOT NULL,
    route_type integer NOT NULL,
    route_colour text NOT NULL,
    route_text_colour text NOT NULL,
    route_url text
);


ALTER TABLE public.routes OWNER TO "user";

--
-- Name: shapes; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.shapes (
    shape_id text NOT NULL,
    shape_pt_lat numeric(9,6) NOT NULL,
    shape_pt_lon numeric(9,6) NOT NULL,
    shape_geom public.geography(Point,4326) NOT NULL,
    shape_pt_sequence integer NOT NULL,
    shape_dist_travelled numeric(9,2),
    route_type integer
);


ALTER TABLE public.shapes OWNER TO "user";

--
-- Name: stop_time_updates; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.stop_time_updates (
    trip_id text NOT NULL,
    stop_id text NOT NULL,
    stop_arrival_time timestamp with time zone,
    stop_arrival_delay bigint,
    stop_departure_time timestamp with time zone,
    stop_departure_delay bigint,
    "timestamp" timestamp with time zone
);


ALTER TABLE public.stop_time_updates OWNER TO "user";

--
-- Name: stop_time_updates_with_progress; Type: VIEW; Schema: public; Owner: user
--

CREATE VIEW public.stop_time_updates_with_progress AS
 SELECT stu.trip_id,
    stu.stop_id,
    st.arrival_time,
    st.departure_time,
    stu.stop_arrival_delay,
    stu.stop_departure_delay,
    stu."timestamp",
        CASE
            WHEN ((st.pickup_type = 1) AND (st.drop_off_type = 1)) THEN 'skipped'::text
            WHEN (((EXTRACT(epoch FROM (now() AT TIME ZONE 'Australia/Sydney'::text)))::integer % 86400) > (st.departure_time + COALESCE(stu.stop_departure_delay, (0)::bigint))) THEN 'passed'::text
            ELSE 'not_passed'::text
        END AS progress
   FROM (public.stop_time_updates stu
     JOIN public.stop_times st ON (((stu.trip_id = st.trip_id) AND (stu.stop_id = st.stop_id))))
  ORDER BY st.arrival_time;


ALTER VIEW public.stop_time_updates_with_progress OWNER TO "user";

--
-- Name: trip_updates; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.trip_updates (
    trip_id text NOT NULL,
    trip_route_id text NOT NULL,
    trip_schedule_relationship text NOT NULL,
    vehicle_id text,
    vehicle_label text,
    vehicle_model text NOT NULL,
    "timestamp" timestamp with time zone NOT NULL,
    route_type integer
);


ALTER TABLE public.trip_updates OWNER TO "user";

--
-- Name: vehicle_boardings; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.vehicle_boardings (
    vehicle_category_id text NOT NULL,
    child_sequence integer NOT NULL,
    grandchild_sequence integer,
    boarding_area_id text NOT NULL
);


ALTER TABLE public.vehicle_boardings OWNER TO "user";

--
-- Name: vehicle_categories; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.vehicle_categories (
    vehicle_category_id text NOT NULL,
    vehicle_category_name text NOT NULL
);


ALTER TABLE public.vehicle_categories OWNER TO "user";

--
-- Name: vehicle_couplings; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.vehicle_couplings (
    parent_id text NOT NULL,
    child_id text NOT NULL,
    child_sequence integer NOT NULL,
    child_label text NOT NULL
);


ALTER TABLE public.vehicle_couplings OWNER TO "user";

--
-- Name: vehicle_positions; Type: TABLE; Schema: public; Owner: user
--

CREATE TABLE public.vehicle_positions (
    trip_id text NOT NULL,
    trip_route_id text NOT NULL,
    trip_schedule_relationship text NOT NULL,
    vehicle_id text NOT NULL,
    vehicle_label text NOT NULL,
    vehicle_model text NOT NULL,
    position_latitude numeric(9,6) NOT NULL,
    position_longitude numeric(9,6) NOT NULL,
    position_geom public.geography(Point,4326) NOT NULL,
    stop_id text NOT NULL,
    "timestamp" timestamp with time zone NOT NULL,
    congestion_level text NOT NULL,
    occupancy_status text,
    route_type integer
);


ALTER TABLE public.vehicle_positions OWNER TO "user";

--
-- Name: agencies agencies_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.agencies
    ADD CONSTRAINT agencies_pkey PRIMARY KEY (agency_id);


--
-- Name: calendars calendars_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.calendars
    ADD CONSTRAINT calendars_pkey PRIMARY KEY (service_id);


--
-- Name: consist consist_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.consist
    ADD CONSTRAINT consist_pkey PRIMARY KEY (vehicle_id, position_in_consist, "timestamp", trip_id);


--
-- Name: notes notes_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.notes
    ADD CONSTRAINT notes_pkey PRIMARY KEY (note_id);


--
-- Name: routes routes_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.routes
    ADD CONSTRAINT routes_pkey PRIMARY KEY (route_id);


--
-- Name: shapes shapes_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.shapes
    ADD CONSTRAINT shapes_pkey PRIMARY KEY (shape_id, shape_pt_sequence);


--
-- Name: stop_time_updates stop_time_updates_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.stop_time_updates
    ADD CONSTRAINT stop_time_updates_pkey PRIMARY KEY (trip_id, stop_id);


--
-- Name: stop_times stop_times_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.stop_times
    ADD CONSTRAINT stop_times_pkey PRIMARY KEY (trip_id, stop_sequence);


--
-- Name: stops stops_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.stops
    ADD CONSTRAINT stops_pkey PRIMARY KEY (stop_id);


--
-- Name: trip_updates trip_updates_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.trip_updates
    ADD CONSTRAINT trip_updates_pkey PRIMARY KEY (trip_id);


--
-- Name: trips trips_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.trips
    ADD CONSTRAINT trips_pkey PRIMARY KEY (trip_id);


--
-- Name: vehicle_boardings vehicle_boardings_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.vehicle_boardings
    ADD CONSTRAINT vehicle_boardings_pkey PRIMARY KEY (vehicle_category_id, child_sequence, boarding_area_id);


--
-- Name: vehicle_categories vehicle_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.vehicle_categories
    ADD CONSTRAINT vehicle_categories_pkey PRIMARY KEY (vehicle_category_id);


--
-- Name: vehicle_couplings vehicle_couplings_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.vehicle_couplings
    ADD CONSTRAINT vehicle_couplings_pkey PRIMARY KEY (parent_id, child_id, child_sequence);


--
-- Name: vehicle_positions vehicle_positions_pkey; Type: CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.vehicle_positions
    ADD CONSTRAINT vehicle_positions_pkey PRIMARY KEY (vehicle_id);


--
-- Name: idx_asd_parent_stop_time; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_asd_parent_stop_time ON public.active_stop_departures USING btree (stop_parent_station, departure_time, start_date, end_date) INCLUDE (stop_id, trip_id, trip_headsign, service_id, stop_name, arrival_time, pickup_type, drop_off_type, monday, tuesday, wednesday, thursday, friday, saturday, sunday);


--
-- Name: idx_asd_unique; Type: INDEX; Schema: public; Owner: user
--

CREATE UNIQUE INDEX idx_asd_unique ON public.active_stop_departures USING btree (trip_id, stop_sequence);


--
-- Name: idx_calendars_date_range; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_calendars_date_range ON public.calendars USING btree (start_date, end_date);


--
-- Name: idx_stop_times_stop_departure; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_stop_times_stop_departure ON public.stop_times USING btree (stop_id, departure_time) INCLUDE (trip_id, arrival_time, pickup_type, drop_off_type);


--
-- Name: idx_stop_times_stop_id; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_stop_times_stop_id ON public.stop_times USING btree (stop_id);


--
-- Name: idx_stop_times_trip_id; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_stop_times_trip_id ON public.stop_times USING btree (trip_id);


--
-- Name: idx_stop_times_trip_stop; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_stop_times_trip_stop ON public.stop_times USING btree (trip_id, stop_id) INCLUDE (arrival_time, departure_time, pickup_type, drop_off_type);


--
-- Name: idx_trips_route_id; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_trips_route_id ON public.trips USING btree (route_id);


--
-- Name: idx_trips_service_id; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX idx_trips_service_id ON public.trips USING btree (service_id) INCLUDE (trip_id, trip_headsign, route_id);


--
-- Name: stops_stop_parent_station_idx; Type: INDEX; Schema: public; Owner: user
--

CREATE INDEX stops_stop_parent_station_idx ON public.stops USING btree (stop_parent_station);


--
-- Name: consist consist_vehicle_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.consist
    ADD CONSTRAINT consist_vehicle_id_fkey FOREIGN KEY (vehicle_id) REFERENCES public.vehicle_positions(vehicle_id) ON DELETE CASCADE;


--
-- Name: routes routes_agency_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.routes
    ADD CONSTRAINT routes_agency_id_fkey FOREIGN KEY (agency_id) REFERENCES public.agencies(agency_id) ON DELETE CASCADE;


--
-- Name: stop_time_updates stop_time_updates_trip_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.stop_time_updates
    ADD CONSTRAINT stop_time_updates_trip_id_fkey FOREIGN KEY (trip_id) REFERENCES public.trip_updates(trip_id) ON DELETE CASCADE;


--
-- Name: stop_times stop_times_stop_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.stop_times
    ADD CONSTRAINT stop_times_stop_id_fkey FOREIGN KEY (stop_id) REFERENCES public.stops(stop_id);


--
-- Name: stop_times stop_times_trip_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.stop_times
    ADD CONSTRAINT stop_times_trip_id_fkey FOREIGN KEY (trip_id) REFERENCES public.trips(trip_id) ON DELETE CASCADE;


--
-- Name: trips trips_route_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.trips
    ADD CONSTRAINT trips_route_id_fkey FOREIGN KEY (route_id) REFERENCES public.routes(route_id) ON DELETE CASCADE;


--
-- Name: vehicle_boardings vehicle_boardings_vehicle_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.vehicle_boardings
    ADD CONSTRAINT vehicle_boardings_vehicle_category_id_fkey FOREIGN KEY (vehicle_category_id) REFERENCES public.vehicle_categories(vehicle_category_id);


--
-- Name: vehicle_couplings vehicle_couplings_child_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.vehicle_couplings
    ADD CONSTRAINT vehicle_couplings_child_id_fkey FOREIGN KEY (child_id) REFERENCES public.vehicle_categories(vehicle_category_id);


--
-- Name: vehicle_couplings vehicle_couplings_parent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user
--

ALTER TABLE ONLY public.vehicle_couplings
    ADD CONSTRAINT vehicle_couplings_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES public.vehicle_categories(vehicle_category_id);


--
-- PostgreSQL database dump complete
--

\unrestrict k8wcMpRG3qnRYkEaRJ0BVNN8ugmu35yVbUSRui5O3WA8525XxLjn6jN3SvK0lnI

