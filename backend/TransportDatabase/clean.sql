-- remove duplicates of nswtrains from sydneytrains
-- to fix: drop_off_type and pickup_type no longer working so possible need to use differences between departure and arrival times to indicate terminating service
DELETE FROM trips WHERE route_id LIKE 'CTY%';
DELETE FROM trips WHERE route_id LIKE 'SHL%';
DELETE FROM trips t USING routes r WHERE t.route_id = r.route_id AND t.trip_id LIKE '%N.2%' AND r.route_type = 2;
DELETE FROM trips t USING routes r WHERE t.route_id = r.route_id AND t.trip_id LIKE '%J.2%' AND r.route_type = 2;