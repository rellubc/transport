FROM postgis/postgis:18-3.6

COPY ./static.sql /docker-entrypoint-initdb.d/
COPY ./realtime.sql /docker-entrypoint-initdb.d/