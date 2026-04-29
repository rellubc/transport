DELETE FROM calendars WHERE start_date < (NOW() AT TIME ZONE 'Australia/Sydney')::date;
DELETE FROM trip_updates WHERE timestamp < (NOW() AT TIME ZONE 'Australia/Sydney')::date;
DELETE FROM vehicle_positions WHERE timestamp < (NOW() AT TIME ZONE 'Australia/Sydney')::date;