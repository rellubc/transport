DELETE FROM calendars WHERE start_date < CURRENT_DATE;
DELETE FROM trip_updates WHERE timestamp < CURRENT_DATE;
DELETE FROM vehicle_positions WHERE timestamp < CURRENT_DATE;