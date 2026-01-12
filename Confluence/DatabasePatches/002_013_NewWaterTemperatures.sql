--Water Temp 20
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:ac_gwtemp20_2015_fl&outputFormat=application/json'
where typeofdata = 'mean_water_temp_20'

--Water Temp 40
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:bc_gwtemp40_2015_fl&outputFormat=application/json'
where typeofdata = 'mean_water_temp_40'

--Water Temp 60
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:cc_gwtemp60_2015_fl&outputFormat=application/json'
where typeofdata = 'mean_water_temp_60'

--Water Temp 80
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:dc_gwtemp80_2015_fl&outputFormat=application/json'
where typeofdata = 'mean_water_temp_80'

--Water Temp 100
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:ec_gwtemp100_2015_fl&outputFormat=application/json'
where typeofdata = 'mean_water_temp_20to100'

UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:ec_gwtemp100_2015_fl&outputFormat=application/json'
where typeofdata = 'mean_water_temp_20to100'