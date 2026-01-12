-- area_usage

ALTER TYPE typeofdata ADD VALUE 'area_usage';

--depth restrictions Berlin (Holstien)
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','area_usage','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_alkis_tatsaechlichenutzungflaechen?REQUEST=GetCapabilities&SERVICE=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_alkis_tatsaechlichenutzungflaechen&outputFormat=application/json','restrictive');