--Water Temp 20to100
UPDATE geothermal_parameter
SET getrequest = 'https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s02_14_05gwtemp_100m_2020?service=wfs&version=2.0.0&request=GetFeature&request=GetFeature&typeNames=fis:s02_14_05gwtemp_100m_2020&outputFormat=application/json'
where typeofdata = 'mean_water_temp_20to100'

--ground_water_height_main
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_grundwassergl_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassergl_2020:bb_hgwl_gwms_li&outputFormat=application/json'
where typeofdata = 'ground_water_height_main'


--water_protec_areas  wsg:wsg_be -> wsg:wsg
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/wsg?service=wfs&version=2.0.0&request=GetFeature&typeNames=wsg:wsg&outputFormat=application/json'
where typeofdata = 'water_protec_areas'

--water_protec_areas  wsg:wsg_be -> wsg:wsg
UPDATE geothermal_parameter
SET getrequest = 'https://gdi.berlin.de/services/wfs/ua_zehgw?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_zehgw:ca_zehgw_linien&outputFormat=application/json'
where typeofdata = 'expe_max_groundwater_hight'


