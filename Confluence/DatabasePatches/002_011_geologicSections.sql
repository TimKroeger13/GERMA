ALTER TYPE typeofdata ADD VALUE 'holstein_restrictions';
ALTER TYPE typeofdata ADD VALUE 'geologic_sections_berlin';

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('holstein_restrictions','berlin','near_range','holstein_restrictions.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('geologic_sections_berlin','berlin','near_range','geologic_sections_berlin.geojson','efficiency','multipolygon');


--Delete old Data
DELETE FROM geo_data
WHERE parameter_key IN (
    SELECT id FROM geothermal_parameter
    WHERE typeofdata = 'depth_restrictions'
);

DELETE FROM geothermal_parameter
WHERE typeofdata = 'depth_restrictions';


DELETE FROM geo_data
WHERE parameter_key IN (
    SELECT id FROM geothermal_parameter
    WHERE typeofdata = 'depth_restrictions_rup'
);

DELETE FROM geothermal_parameter
WHERE typeofdata = 'depth_restrictions_rup';