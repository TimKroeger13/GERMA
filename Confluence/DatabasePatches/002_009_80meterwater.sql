ALTER TYPE typeofdata ADD VALUE 'mean_water_temp_80';

--Water Temp 80
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('mean_water_temp_80','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2020:db_gwtemp80_2020&outputFormat=application/json','efficiency','multipolygon');


ALTER TYPE typeofdata ADD VALUE 'depth_restrictions_rup';


INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
    VALUES ('depth_restrictions_rup','berlin','near_range','rup_depth.geojson','efficiency','multipolygon');


