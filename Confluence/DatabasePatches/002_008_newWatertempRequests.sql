--Water Temp 20to100
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2020:eb_gwtemp100_2020&outputFormat=application/json'
where typeofdata = 'mean_water_temp_20to100'

--Water Temp 20
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2020:ab_gwtemp20_2020&outputFormat=application/json'
where typeofdata = 'mean_water_temp_20'

--Water Temp 40
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2020:bb_gwtemp40_2020&outputFormat=application/json'
where typeofdata = 'mean_water_temp_40'

--Water Temp 60
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2020:cb_gwtemp60_2020&outputFormat=application/json'
where typeofdata = 'mean_water_temp_60'

