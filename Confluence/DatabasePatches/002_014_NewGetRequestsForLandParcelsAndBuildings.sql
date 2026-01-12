--Flurstücke
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/alkis_flurstuecke?service=wfs&version=2.0.0&request=GetFeature&typeNames=alkis_flurstuecke:flurstuecke&outputFormat=application/json'
where typeofdata = 'land_parcels'

--Gebäude
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/alkis_gebaeude?service=wfs&version=2.0.0&request=GetFeature&typeNames=alkis_gebaeude:gebaeude&outputFormat=application/json'
where typeofdata = 'building_surfaces'