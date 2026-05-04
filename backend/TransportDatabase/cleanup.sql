DELETE FROM calendars WHERE start_date < (NOW() AT TIME ZONE 'Australia/Sydney')::date;
DELETE FROM trip_updates WHERE timestamp < (NOW() AT TIME ZONE 'Australia/Sydney')::date;
DELETE FROM vehicle_positions WHERE timestamp < (NOW() AT TIME ZONE 'Australia/Sydney')::date;

REFRESH MATERIALIZED VIEW active_stop_departures;
VACUUM active_stop_departures;
ANALYZE active_stop_departures;