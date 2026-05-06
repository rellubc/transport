import os
import requests
import zipfile
import io

from pathlib import Path
from dotenv import load_dotenv

BASE_DIR = Path(__file__).resolve().parent.parent
ENV_PATH = BASE_DIR / ".env"
load_dotenv(ENV_PATH)

API_KEY = os.getenv("API_KEY", "")

API_KEY = os.getenv("API_KEY", "")

V2_URL = "https://api.transport.nsw.gov.au/v2/gtfs/schedule/"
V1_URL = "https://api.transport.nsw.gov.au/v1/gtfs/schedule/"

V2_MODES = [
    "metro",
]

V1_MODES = [
    "sydneytrains",
    "nswtrains",

    "lightrail/newcastle",
    "lightrail/innerwest",
    "lightrail/parramatta",
    "lightrail/cbdandsoutheast",

    "ferries/sydneyferries",
    "ferries/MFF",

    "buses/SBSC006",
    "buses/GSBC001",
    "buses/GSBC002",
    "buses/GSBC003",
    "buses/GSBC004",
    "buses/GSBC007",
    "buses/GSBC008",
    "buses/GSBC009",
    "buses/GSBC010",
    "buses/GSBC014",
]

# V2_MODES = {
#     "metro": 401,
# }

# V1_MODES = {
#     "sydneytrains": 2,
#     "nswtrains": 100,

#     "lightrail/newcastle": 0,
#     "lightrail/innerwest": 900,
#     "lightrail/parramatta": 900,
#     "lightrail/cbdandsoutheast": 900,

#     "ferries/sydneyferries": 4,
#     "ferries/MFF": 4,

#     "buses/SBSC006",
#     "buses/GSBC001",
#     "buses/GSBC002",
#     "buses/GSBC003",
#     "buses/GSBC004",
#     "buses/GSBC007",
#     "buses/GSBC008",
#     "buses/GSBC009",
#     "buses/GSBC010",
#     "buses/GSBC014",
# }

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

        output_dir = f"./._gtfs/{feed_version}/{mode_string}"
        os.makedirs(output_dir, exist_ok=True)
        zip_file.extractall(output_dir)
        print(f"Extracted {url}{mode_string} to {output_dir}")