UPDATE geothermal_parameter
SET getrequest = 'groundwater_measuring_points.geojson'
where typeofdata = 'groundwater_measuring_points'

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('geodrilling_data','berlin','near_range','drilling_points.geojson','restrictive','point');
