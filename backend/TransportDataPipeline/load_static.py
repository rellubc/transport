import os
import requests
import zipfile
import io
import csv

import psycopg2
from psycopg2.extras import execute_values

from pathlib import Path
from dotenv import load_dotenv

BASE_DIR = Path(__file__).resolve().parent.parent
ENV_PATH = BASE_DIR / ".env"
load_dotenv(ENV_PATH)

API_KEY = os.getenv("API_KEY", "")

POSTGRES_HOST = os.getenv("POSTGRES_HOST", "localhost")
POSTGRES_PORT = os.getenv("POSTGRES_PORT", "5432")
POSTGRES_DB = os.getenv("POSTGRES_DB", "transport_db")
POSTGRES_USER = os.getenv("POSTGRES_USER", "postgres")
POSTGRES_PASSWORD = os.getenv("POSTGRES_PASSWORD", "password")

V2_URL = "https://api.transport.nsw.gov.au/v2/gtfs/schedule/"
V1_URL = "https://api.transport.nsw.gov.au/v1/gtfs/schedule/"

V2_MODES = {
    "metro": 401,
}

V1_MODES = {
    "sydneytrains": 2,
    "nswtrains": 100,

    "lightrail/newcastle": 0,
    "lightrail/innerwest": 900,
    "lightrail/parramatta": 900,
    "lightrail/cbdandsoutheast": 900,

    "ferries/sydneyferries": 4,
    "ferries/MFF": 4,

    # "buses/SBSC006",
    # "buses/GSBC001",
    # "buses/GSBC002",
    # "buses/GSBC003",
    # "buses/GSBC004",
    # "buses/GSBC007",
    # "buses/GSBC008",
    # "buses/GSBC009",
    # "buses/GSBC010",
    # "buses/GSBC014",
}

MAPPINGS = {
    "agency.txt": ("agencies", { 
        "agency_id": "agency_id", 
        "agency_name": "agency_name",
        "agency_url": "agency_url",
        "agency_timezone": "agency_timezone",
        "agency_lang": "agency_language",
        "agency_phone": "agency_phone",
        "agency_fare_url": "agency_fare_url",
        "agency_email": "agency_email" 
    }),
    "calendar.txt": ("calendars", {
        "service_id": "service_id",
        "monday": "monday",
        "tuesday": "tuesday",
        "wednesday": "wednesday",
        "thursday": "thursday",
        "friday": "friday",
        "saturday": "saturday",
        "sunday": "sunday",
        "start_date": "start_date",
        "end_date": "end_date"
    }),
    "notes.txt": ("notes", {
        "note_id": "note_id",
        "note_text": "note_text"
    }),
    "routes.txt": ("routes", {
        "route_id": "route_id",
        "agency_id": "agency_id",
        "route_short_name": "route_short_name",
        "route_long_name": "route_long_name",
        "route_desc": "route_desc",
        "route_type": "route_type",
        "route_color": "route_colour",
        "route_text_color": "route_text_colour",
        "route_url": "route_url"
    }),
    "shapes.txt": ("shapes", {
        "shape_id": "shape_id",
        "shape_pt_lat": "shape_pt_lat",
        "shape_pt_lon": "shape_pt_lon",
        "shape_geom": "shape_geom",
        "shape_pt_sequence": "shape_pt_sequence",
        "shape_dist_travelled": "shape_dist_travelled",
        "route_type": "route_type",
    }),
    "stops.txt": ("stops", {
        "stop_id": "stop_id",
        "stop_code": "stop_code",
        "stop_name": "stop_name",
        "stop_desc": "stop_desc",
        "stop_lat": "stop_lat",
        "stop_lon": "stop_lon",
        "stop_geom": "stop_geom",
        "zone_id": "stop_zone_id",
        "stop_url": "stop_url",
        "location_type": "stop_location_type",
        "parent_station": "stop_parent_station",
        "stop_timezone": "stop_timezone",
        "wheelchair_boarding": "stop_wheelchair_boarding",
        "platform_code": "stop_platform_code",
        "route_type": "route_type",
    }),
    "vehicle_categories.txt": ("vehicle_categories", {
        "vehicle_category_id": "vehicle_category_id",
        "vehicle_category_name": "vehicle_category_name"
    }),
    "trips.txt": ("trips", {
        "route_id": "route_id",
        "service_id": "service_id",
        "trip_id": "trip_id",
        "shape_id": "shape_id",
        "trip_headsign": "trip_headsign",
        "direction_id": "trip_direction_id",
        "trip_short_name": "trip_short_name",
        "block_id": "trip_block_id",
        "wheelchair_accessible": "trip_wheelchair_accessible",
        "trip_note": "trip_note",
        "route_direction": "trip_route_direction",
        "bikes_allowed": "trip_bikes_allowed"
    }),
    "stop_times.txt": ("stop_times", {
        "trip_id": "trip_id",
        "arrival_time": "arrival_time",
        "departure_time": "departure_time",
        "stop_id": "stop_id",
        "stop_sequence": "stop_sequence",
        "stop_headsign": "stop_headsign",
        "pickup_type": "pickup_type",
        "drop_off_type": "drop_off_type",
        "shape_dist_travelled": "shape_dist_travelled",
        "timepoint": "timepoint",
        "stop_note": "stop_note",
        "route_type": "route_type",
    }),
    "vehicle_boardings.txt": ("vehicle_boardings", {
        "vehicle_category_id": "vehicle_category_id",
        "child_sequence": "child_sequence",
        "grandchild_sequence": "grandchild_sequence",
        "boarding_area_id": "boarding_area_id"
    }),
    "vehicle_couplings.txt": ("vehicle_couplings", {
        "parent_id": "parent_id",
        "child_id": "child_id",
        "child_sequence": "child_sequence",
        "child_label": "child_label"
    }),
}

TYPE_CASTS = {
    "stop_lat": float,
    "stop_lon": float,
    "shape_pt_lat": float,
    "shape_pt_lon": float,
    "shape_pt_sequence": int,
    "stop_sequence": int,
    "arrival_time": str,
    "departure_time": str,
    "route_type": int,
    "direction_id": int,
    "wheelchair_boarding": int,
    "wheelchair_accessible": int,
    "bikes_allowed": int,
    "timepoint": int,
    "service_id": str,
}

def connect_db():
    print("Connect to db...")
    conn = psycopg2.connect(
        host=POSTGRES_HOST,
        port=POSTGRES_PORT,
        dbname=POSTGRES_DB,
        user=POSTGRES_USER,
        password=POSTGRES_PASSWORD
    )

    return conn

def truncate_tables(conn):
    print("Truncating tables...")
    with conn.cursor() as cur:
        cur.execute("""
            TRUNCATE TABLE
                agencies,
                calendars,
                notes,
                routes,
                stops,
                stop_times,
                trips,
                vehicle_boardings,
                vehicle_categories,
                vehicle_couplings
            RESTART IDENTITY CASCADE;
        """)

    print("Tables truncated.")

def drop_indexes(conn):
    print("Dropping indexes...")
    with conn.cursor() as cur:
        cur.execute("""
            DROP INDEX IF EXISTS idx_asd_parent_stop_time;
            DROP INDEX IF EXISTS idx_asd_unique;

            DROP INDEX IF EXISTS idx_stop_times_stop_departure;
            DROP INDEX IF EXISTS idx_stop_times_stop_id;
            DROP INDEX IF EXISTS idx_stop_times_trip_id;
            DROP INDEX IF EXISTS idx_stop_times_trip_stop;

            DROP INDEX IF EXISTS idx_trips_route_id;
            DROP INDEX IF EXISTS idx_trips_service_id;

            DROP INDEX IF EXISTS stops_stop_parent_station_idx;

            DROP INDEX IF EXISTS idx_calendars_date_range;
        """)
    
    print("Indexes dropped.")

def insert_static_data(conn):
    headers = {
        "Authorization": f"apikey {API_KEY}"
    }

    for url, modes in [(V2_URL, V2_MODES), (V1_URL, V1_MODES)]:
        feed_version = url.split("/")[3].upper()
        print(f"Fetching Sydney Transport {feed_version} static data...")

        for mode_string in modes:
            try:
                r = requests.get(f"{url}{mode_string}", headers=headers)
                if r.status_code == 404:
                    print(f"Skipping {url}{mode_string} (404)...")
                    continue
                r.raise_for_status()
            except requests.HTTPError as e:
                print(f"Skipping {url}{mode_string} ({e})...")
                continue

            zip_file = zipfile.ZipFile(io.BytesIO(r.content))
            
            print(f"Fetching {mode_string} static data...")

            for filename in zip_file.namelist():
                if filename not in MAPPINGS.keys():
                    print(f"Unused {filename}...")
            
            for filename, (table, columns) in MAPPINGS.items():
                if filename not in zip_file.namelist():
                    print(f"Skipping {filename}...")
                    continue

                if filename == "shapes.txt":
                    continue

                print(f"Loading {filename}...")
                with zip_file.open(filename) as file:
                    conflict_key_map = {
                        "agency.txt": ["agency_id"],
                        "calendar.txt": ["service_id"],
                        "notes.txt": ["note_id"],
                        "routes.txt": ["route_id"],
                        "stop_times.txt": ["trip_id", "stop_sequence"],
                        "stops.txt": ["stop_id", "route_type"],
                        "trips.txt": ["trip_id"],
                        "vehicle_categories.txt": ["vehicle_category_id"],
                        "vehicle_boardings.txt": ["vehicle_category_id", "child_sequence", "boarding_area_id"],
                        "vehicle_couplings.txt": ["parent_id", "child_id", "child_sequence"],
                    }
                    conflict_key = conflict_key_map.get(filename, [])

                    print(file.readline().decode('utf-8-sig').strip())
                    file.seek(0)
                    
                    mode_num = V2_MODES.get(mode_string) or V1_MODES.get(mode_string)
        
                    load(conn, file, table, columns, conflict_key, mode_num)

    print(f"Loaded Sydney Transport {url.split("/")[4]} static data")

def insert_shapes(conn):
    shapes_folder = f"{os.getcwd()}/._shapes"
    conflict_key = ["shape_id", "shape_pt_sequence"]

    for [mode_string, mode_num] in V1_MODES.items():
        print(mode_string, mode_num)
        os.makedirs(f"{shapes_folder}/{mode_string}", exist_ok=True)
        for filename in os.listdir(f"{shapes_folder}/{mode_string}"):
            print(f"Loading {filename}...")
            with open(f"{shapes_folder}/{mode_string}/{filename}", "rb") as file:
                load(conn, file, MAPPINGS["shapes.txt"][0], MAPPINGS["shapes.txt"][1], conflict_key, mode_num)

    for [mode_string, mode_num] in V2_MODES.items():
        os.makedirs(f"{shapes_folder}/{mode_string}", exist_ok=True)
        for filename in os.listdir(f"{shapes_folder}/{mode_string}"):
            print(f"Loading {filename}...")
            with open(f"{shapes_folder}/{mode_string}/{filename}", "rb") as file:
                load(conn, file, MAPPINGS["shapes.txt"][0], MAPPINGS["shapes.txt"][1], conflict_key, mode_num)

def refresh_materialised_views(conn):
    print("Refreshing...")
    with conn.cursor() as cur:
        cur.execute("""
            DELETE FROM calendars WHERE start_date < (NOW() AT TIME ZONE 'Australia/Sydney')::date;
            DELETE FROM trip_updates WHERE timestamp < (NOW() AT TIME ZONE 'Australia/Sydney')::date;
            DELETE FROM vehicle_positions WHERE timestamp < (NOW() AT TIME ZONE 'Australia/Sydney')::date;
            REFRESH MATERIALIZED VIEW active_stop_departures;
        """)
    conn.commit()

    conn.autocommit = True
    with conn.cursor() as cur:
        cur.execute("VACUUM ANALYZE active_stop_departures;")
    conn.autocommit = False
    conn.commit()

def create_indexes(conn):
    print("Creating indexes...")

    conn.autocommit = True
    with conn.cursor() as cur:
        cur.execute("SET maintenance_work_mem = '1GB';")
    conn.autocommit = False

    with conn.cursor() as cur:
        cur.execute("""
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
        """)
    conn.commit()

def clean_data(conn):
    print("Cleaning data...")
    with conn.cursor() as cur:
        cur.execute("""
            DELETE FROM trips WHERE route_id LIKE 'CTY%';
            DELETE FROM trips WHERE route_id LIKE 'SHL%';
            DELETE FROM trips t USING routes r WHERE t.route_id = r.route_id AND t.trip_id LIKE '%N.2%' AND r.route_type = 2;
            DELETE FROM trips t USING routes r WHERE t.route_id = r.route_id AND t.trip_id LIKE '%N.4%' AND r.route_type = 2;
            DELETE FROM trips t USING routes r WHERE t.route_id = r.route_id AND t.trip_id LIKE '%J.2%' AND r.route_type = 2;
        """)
    conn.commit()

def time_to_seconds(s):
    if not s:
        return None
    h, m, sec = map(int, s.split(":"))
    return h * 3600 + m * 60 + sec

def load(conn, file, table_name, column_map, conflict_key, mode_num, batch_size=10000):
    reader = csv.DictReader(io.TextIOWrapper(file, "utf-8-sig"))
    
    colnames = list(column_map.values())
    updates = ", ".join([f"{col}=EXCLUDED.{col}" for col in colnames if col not in conflict_key])
    query = f"""
        INSERT INTO {table_name} ({', '.join(colnames)})
        VALUES %s
        ON CONFLICT ({', '.join(conflict_key)})
        DO NOTHING
    """

    batch = []
    with conn.cursor() as cur:
        for row in reader:
            values = []
            lat = row.get("shape_pt_lat") or row.get("stop_lat")
            lon = row.get("shape_pt_lon") or row.get("stop_lon")

            for file_col, db_col in column_map.items():
                val = row.get(file_col, None)

                if val == "":
                    val = None
                elif db_col in TYPE_CASTS and val is not None:
                    try:
                        val = TYPE_CASTS[db_col](val)
                    except:
                        val = None

                if db_col == "stop_name":
                    val = re.sub(r'([A-Z]\w+) Platform (\d+)$', r'\1, Platform \2', val)

                if file.name != "routes.txt" and db_col == "route_type":
                    val = mode_num

                if "geom" in db_col:
                    val = f"SRID=4326;POINT({lon} {lat})"

                if db_col in ["arrival_time", "departure_time"]:
                    val = time_to_seconds(val)

                values.append(val)

            batch.append(values)

            if len(batch) >= batch_size:
                execute_values(cur, query, batch, page_size=1000)
                conn.commit()
                batch.clear()

        if batch:
            execute_values(cur, query, batch, page_size=1000)
            conn.commit()

def main():
    conn = connect_db()

    # full delete
    # drop_indexes(conn)
    # truncate_tables(conn)
    # conn.commit()

    # full ingestion
    insert_static_data(conn)
    insert_shapes(conn)

    # refresh
    refresh_materialised_views(conn)
    create_indexes(conn)

    # clean
    clean_data(conn)

    print("Static data loaded")
    conn.close()

if __name__ == "__main__":
    main()