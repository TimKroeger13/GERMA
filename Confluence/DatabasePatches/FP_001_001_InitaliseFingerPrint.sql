CREATE TYPE finger_print_types AS ENUM ('session_loaded','session_ended','initial_request','report_request');

CREATE TABLE finger_print (
id SERIAL PRIMARY KEY NOT NULL,
userid TEXT,
finger_print_types finger_print_types,
geom geometry,
time TIMESTAMP
);