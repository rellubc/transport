CREATE MATERIALIZED VIEW active_stop_departures AS
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
    FROM trips t
        JOIN calendars c ON c.service_id = t.service_id
        JOIN stop_times st ON st.trip_id = t.trip_id
        JOIN stops s ON s.stop_id = st.stop_id
    WITH NO DATA;

CREATE VIEW stop_time_updates_with_progress AS
    SELECT stu.trip_id,
        stu.stop_id,
        st.arrival_time,
        st.departure_time,
        stu.stop_arrival_delay,
        stu.stop_departure_delay,
        stu."timestamp",
            CASE
                WHEN st.pickup_type = 1 AND st.drop_off_type = 1 THEN 'skipped'::text
                WHEN EXTRACT(epoch FROM (now() AT TIME ZONE 'Australia/Sydney'::text))::integer % 86400 > st.departure_time + COALESCE(stu.stop_departure_delay, (0)::bigint) THEN 'passed'::text
                ELSE 'not_passed'::text
            END AS progress
    FROM stop_time_updates stu
        JOIN stop_times st ON stu.trip_id = st.trip_id AND stu.stop_id = st.stop_id
    ORDER BY st.arrival_time;
