import os
import requests
import zipfile
import io
import csv

import psycopg2

from pathlib import Path
from dotenv import load_dotenv

BASE_DIR = Path(__file__).resolve().parent.parent
ENV_PATH = BASE_DIR / ".env"
load_dotenv(ENV_PATH)

API_KEY = os.getenv("API_KEY", "")

DB_HOST = os.getenv("POSTGRES_HOST", "localhost")
DB_PORT = os.getenv("POSTGRES_PORT", "5432")
DB_NAME = os.getenv("POSTGRES_DB", "transport_db")
DB_USER = os.getenv("POSTGRES_USER", "postgres")
DB_PASSWORD = os.getenv("POSTGRES_PASSWORD", "password")

GTFS_URL = "https://api.transport.nsw.gov.au/v2/gtfs/schedule/"

MODES = {
    "metro": 401,
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
        "stop_name": "stop_name",
        "stop_lat": "stop_lat",
        "stop_lon": "stop_lon",
        "stop_geom": "stop_geom",
        "location_type": "stop_location_type",
        "parent_station": "stop_parent_station",
        "wheelchair_boarding": "stop_wheelchair_boarding",
        "platform_code": "stop_platform_code",
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
    "timepoint": int
}

def connect_db():
    print("Connect to db...")
    return psycopg2.connect(
        host=DB_HOST,
        port=DB_PORT,
        dbname=DB_NAME,
        user=DB_USER,
        password=DB_PASSWORD
    )

def time_to_seconds(s):
    if not s:
        return None
    h, m, sec = map(int, s.split(":"))
    return h * 3600 + m * 60 + sec

def load(conn, file, table_name, column_map, conflict_key, mode_string='', batch_size=10000):
    reader = csv.DictReader(io.TextIOWrapper(file, "utf-8-sig"))
    
    colnames = list(column_map.values())
    placeholders = ", ".join(["%s"] * len(colnames))
    updates = ", ".join([f"{col}=EXCLUDED.{col}" for col in colnames if col not in conflict_key])

    query = f"""
        INSERT INTO {table_name} ({', '.join(colnames)})
        VALUES ({placeholders})
        ON CONFLICT ({', '.join(conflict_key)})
        DO UPDATE SET {updates};
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

                if file.name != "routes.txt" and db_col == "route_type" and mode_string in MODES:
                    val = MODES[mode_string]

                if "geom" in db_col:
                    val = f"SRID=4326;POINT({lon} {lat})"

                if db_col in ["arrival_time", "departure_time"]:
                    val = time_to_seconds(val)

                values.append(val)

            batch.append(values)

            if len(batch) >= batch_size:
                cur.executemany(query, batch)
                conn.commit()
                batch.clear()

        if batch:
            cur.executemany(query, batch)
            conn.commit()

def main():
    print("Fetching Sydney transport V2 static data...")

    headers = {
        "Authorization": f"apikey {API_KEY}"
    }
    
    conn = connect_db()

    for mode_string in MODES:
        r = requests.get(f"{GTFS_URL}{mode_string}", headers=headers)
        r.raise_for_status()
        zip_file = zipfile.ZipFile(io.BytesIO(r.content))
        
        print(f"Fetching {mode_string} static data...")
        
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
                    "stops.txt": ["stop_id"],
                    "trips.txt": ["trip_id"],
                }
                conflict_key = conflict_key_map.get(filename, [])

                print(file.readline().decode('utf-8-sig').strip())
                file.seek(0)
                
                load(conn, file, table, columns, conflict_key)

    with conn.cursor() as cur:
        cur.execute("""
            UPDATE stop_times st
            SET route_type = sub.route_type
            FROM (
                SELECT DISTINCT ON (st.trip_id, st.stop_sequence) st.trip_id, st.stop_sequence, r.route_type
                FROM stop_times st
                JOIN trips t ON st.trip_id = t.trip_id
                JOIN routes r ON t.route_id = r.route_id
            ) sub
            WHERE st.trip_id = sub.trip_id AND st.stop_sequence = sub.stop_sequence;
        """)
        conn.commit()

        cur.execute("""
            UPDATE stops s
            SET route_type = sub.route_type
            FROM (
                SELECT DISTINCT ON (s.stop_id) s.stop_id, st.route_type
                FROM stops s
                JOIN stop_times st ON s.stop_id = st.stop_id
                UNION
                SELECT DISTINCT ON (s.stop_id) s.stop_id, st.route_type
                FROM stops s
                JOIN stops child ON child.stop_parent_station = s.stop_id
                JOIN stop_times st ON child.stop_id = st.stop_id
            ) sub
            WHERE s.stop_id = sub.stop_id;
        """)
        conn.commit()

    shapes_folder = f"{os.getcwd()}/._shapes"
    conflict_key = ["shape_id", "shape_pt_sequence"]

    for mode_string in MODES:
        os.makedirs(f"{shapes_folder}/{mode_string}", exist_ok=True)
        for filename in os.listdir(f"{shapes_folder}/{mode_string}"):
            print(f"Loading {filename}...")
            with open(f"{shapes_folder}/{mode_string}/{filename}", "rb") as file:
                load(conn, file, MAPPINGS["shapes.txt"][0], MAPPINGS["shapes.txt"][1], conflict_key, mode_string)

    conn.close()
    print("Loaded Sydney Metro static data")

if __name__ == "__main__":
    main()