SELECT * FROM geothermal_parameter
ORDER BY id





CREATE TABLE slection_area_control AS
SELECT parameter ->> 'Bezeich',parameter, geom FROM geo_data
WHERE parameter_key = 34 
AND (
    parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche' 
    OR parameter ->> 'Bezeich' = 'AX_IndustrieUndGewerbeflaeche' 
)


CREATE TABLE slection_area_control AS
SELECT parameter ->> 'Bezeich',parameter, geom FROM geo_data
WHERE parameter_key = 34 
AND parameter ->> 'Bezeich' = 'AX_IndustrieUndGewerbeflaeche' 


CREATE TABLE slection_area_control_wohn AS
SELECT parameter ->> 'Bezeich',parameter, geom FROM geo_data
WHERE parameter_key = 34 
AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche' 



DROP TABLE slection_area_control



SELECT DISTINCT parameter ->> 'Bezeich' FROM geo_data
WHERE parameter_key = 34



