DELETE FROM calendars WHERE start_date < CURRENT_DATE;
DELETE FROM stop_time_updates WHERE timestamp < CURRENT_DATE;
DELETE FROM vehicle_positions WHERE timestamp < CURRENT_DATE;