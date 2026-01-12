--delete old data

DELETE FROM geo_data
WHERE parameter_key IN (
    SELECT id
    FROM geothermal_parameter
    WHERE typeofdata = 'tree_points'
);

DELETE
FROM geothermal_parameter
WHERE typeofdata = 'tree_points'


--add new data
ALTER TYPE typeofdata ADD VALUE 'tree_vector';

--tree vector
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree1.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree2.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree3_1_1.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree3_1_2.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree3_1_3.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree3_2.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree3_3.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree4.geojson','restrictive','multipolygon');

    INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('tree_vector','berlin','near_range','tree5.geojson','restrictive','multipolygon');


