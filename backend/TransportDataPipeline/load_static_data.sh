#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LOG_FILE="$SCRIPT_DIR/logs/load_static_data_log.txt"

python3 -m venv "$SCRIPT_DIR/venv"
"$SCRIPT_DIR/venv/bin/pip" install -r "$SCRIPT_DIR/requirements.txt"

mkdir -p "$SCRIPT_DIR/logs"

echo >> "$LOG_FILE"
echo "===== Loading Sydney Transport static data started at $(date) =====" >> "$LOG_FILE" 2>&1

"$SCRIPT_DIR/venv/bin/python" -u "$SCRIPT_DIR/load_static.py" >> "$LOG_FILE" 2>&1
echo "Finished load_static.py at $(date)" >> "$LOG_FILE" 2>&1

echo "===== Loading Sydney Transport static data completed at $(date) =====" >> "$LOG_FILE" 2>&1

# powershell.exe -Command "Start-Sleep -Seconds 5; Add-Type -Assembly System.Windows.Forms; [System.Windows.Forms.Application]::SetSuspendState('Suspend', \$false, \$false)"