-- area_usage

ALTER TYPE typeofdata ADD VALUE 'tree_points';

--Straßenbäume
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','tree_points','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_baumbestand?REQUEST=GetCapabilities&SERVICE=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_baumbestand&outputFormat=application/json','restrictive');


--Anlagenbäume
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','tree_points','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_baumbestand_an?REQUEST=GetCapabilities&SERVICE=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_baumbestand_an&outputFormat=application/json','restrictive');