DROP TABLE geo_data;
DROP TABLE geothermal_parameter;
DROP TYPE typeofdata;
DROP TYPE area;
DROP TYPE range;
DROP TYPE service;
DROP TYPE geometry_type ;

-- Compelte Update

DELETE FROM geo_data;

UPDATE geothermal_parameter
SET hash = 0;