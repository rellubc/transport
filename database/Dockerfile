FROM mysql:oracle

COPY --chown=mysql:mysql ./01-sydney-metro-tables.sql /docker-entrypoint-initdb.d/
# COPY --chown=mysql:mysql ./02-import.sh /docker-entrypoint-initdb.d/
# COPY --chown=mysql:mysql ./data /var/lib/mysql-files/
