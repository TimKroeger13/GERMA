UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/wsg?service=wfs&version=2.0.0&request=GetFeature&typeNames=wsg:wsg_be&outputFormat=application/json'
where typeofdata = 'water_protec_areas'