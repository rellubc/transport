#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LOG_FILE="$SCRIPT_DIR/logs/load_static_data_log.txt"

echo >> "$LOG_FILE"
echo "===== Loading Sydney Transport static data at $(date) =====" >> "$LOG_FILE" 2>&1

# "$SCRIPT_DIR/venv/bin/python" -u "$SCRIPT_DIR/load_static_v2.py" >> "$LOG_FILE" 2>&1
# echo "Finished load_static_v2.py at $(date)" >> "$LOG_FILE" 2>&1

# "$SCRIPT_DIR/venv/bin/python" -u "$SCRIPT_DIR/load_static_v1.py" >> "$LOG_FILE" 2>&1
# echo "Finished load_static_v1.py at $(date)" >> "$LOG_FILE" 2>&1

"$SCRIPT_DIR/venv/bin/python" -u "$SCRIPT_DIR/load_static.py" >> "$LOG_FILE" 2>&1
echo "Finished load_static.py at $(date)" >> "$LOG_FILE" 2>&1

echo "===== ETL completed at $(date) =====" >> "$LOG_FILE" 2>&1