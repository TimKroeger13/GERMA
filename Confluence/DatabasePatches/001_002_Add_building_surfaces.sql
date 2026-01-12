ALTER TYPE typeofdata ADD VALUE 'building_surfaces';

-- Gebäudeflaechen
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('building_surfaces','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_alkis_gebaeudeflaechen?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_alkis_gebaeudeflaechen&outputFormat=application/json','restrictive');