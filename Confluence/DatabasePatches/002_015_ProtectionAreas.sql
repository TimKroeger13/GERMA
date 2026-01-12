ALTER TYPE typeofdata ADD VALUE 'protection_area';

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('protection_area','berlin','near_range','schutzgebiete.geojson','restrictive','multipolygon');