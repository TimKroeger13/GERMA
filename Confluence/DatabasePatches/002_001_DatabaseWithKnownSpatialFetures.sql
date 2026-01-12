--database germa
CREATE TYPE typeofdata AS ENUM ('land_parcels','dgm','geo_poten_restrict','veg_height',
								'main_water_lines','groundwater_surface_distance','ground_water_height_main',
								'ground_water_height_tension','water_ammonium','water_bor','water_chlor',
								'water_kalium','water_sulfat','water_ortho_phosphat','electrical_con',
								'mean_water_temp_20to100','mean_water_temp_20','mean_water_temp_40',
								'mean_water_temp_60','geodrilling_data','geological_sections','geo_drawing',
								'water_protec_areas','expe_max_groundwater_hight','geo_poten_100m_with_2400ha',
								'geo_poten_100m_with_1800ha','geo_poten_80m_with_2400ha','geo_poten_80m_with_1800ha',
								'geo_poten_60m_with_2400ha','geo_poten_60m_with_1800ha','geo_poten_40m_with_2400ha',
								'geo_poten_40m_with_1800ha','thermal_con_40','thermal_con_60','thermal_con_80',
								'thermal_con_100','groundwater_measuring_points','building_surfaces');

CREATE TYPE area AS ENUM ('berlin');

CREATE TYPE range AS ENUM ('near_range','far_range');

CREATE TYPE service AS ENUM ('restrictive','efficiency');

CREATE TYPE geometry_type AS ENUM ('point','polygon','polyline','raster','multipolygon');

CREATE TABLE geothermal_parameter (
id SERIAL PRIMARY KEY NOT NULL,
typeofdata typeofdata,
area area,
range range,
geometry_type geometry_type,
getrequest TEXT,
service service,
srid INT,
last_update TIMESTAMP,
last_ping TIMESTAMP,
hash BIGINT
);

CREATE TABLE geo_data
(
    id SERIAL PRIMARY KEY NOT NULL,
    parameter_key integer NOT NULL,
    geom geometry,
    parameter json,
    CONSTRAINT parameter_key FOREIGN KEY (parameter_key)
        REFERENCES public.geothermal_parameter (id)
);

--Flurstücke
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','land_parcels','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_alkis?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_alkis&outputFormat=application/json','restrictive');


--Entzugsleistung 40 m, für 1800 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_40m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_40&outputFormat=application/json','efficiency');

--Entzugsleistung 40 m, für 2400 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_40m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_40&outputFormat=application/json','efficiency');

--Entzugsleistung 60 m, für 1800 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_60m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_60&outputFormat=application/json','efficiency');

--Entzugsleistung 60 m, für 2400 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_60m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_60&outputFormat=application/json','efficiency');

--Entzugsleistung 80 m, für 1800 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_80m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_80&outputFormat=application/json','efficiency');

--Entzugsleistung 80 m, für 2400 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_80m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_80&outputFormat=application/json','efficiency');

--Entzugsleistung 100 m, für 2400 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_100m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_100&outputFormat=application/json','efficiency');

--Entzugsleistung 100 m, für 1800 h/a
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','geo_poten_100m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_100&outputFormat=application/json','efficiency');


--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 40 m
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','thermal_con_40','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_40&outputFormat=application/json','efficiency');

--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 60 m
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','thermal_con_60','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_60&outputFormat=application/json','efficiency');

--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 80 m
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','thermal_con_80','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_80&outputFormat=application/json','efficiency');

--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 100 m
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','thermal_con_100','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_100&outputFormat=application/json','efficiency');


--Geothermisches Potenzial - Restriktionsflächen
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('multipolygon','geo_poten_restrict','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_02_18_restrict_2017?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_02_18_restrict_2017&outputFormat=application/json','restrictive');


--Flächen Hauptgrundwasserleiter gespannt
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('multipolygon','groundwater_surface_distance','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_flurabstand_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_flurabstand_2020:a_panketal&outputFormat=application/json','efficiency');


--Grundwassergleichen 2020 - Hauptgrundwasserleiter
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polyline','ground_water_height_main','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_gwgleichen_haupt?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_gwgleichen_haupt&outputFormat=application/json','efficiency');

--Grundwassergleichen 2020 - Panketalgrundwasserleiter
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polyline','ground_water_height_tension','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_gwgleichen_panke?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_gwgleichen_panke&outputFormat=application/json','efficiency');


-- Grundwassergüte Ammonium
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','water_ammonium','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_4amm?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_4amm&outputFormat=application/json','restrictive');

-- Grundwassergüte Bor
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','water_bor','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_8bor?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_8bor&outputFormat=application/json','restrictive');

-- Grundwassergüte Chlor
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','water_chlor','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_2chlo?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_2chlo&outputFormat=application/json','restrictive');

-- Grundwassergüte Elektrische Leitfähigkeit
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','electrical_con','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_1leit?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_1leit&outputFormat=application/json','restrictive');

-- Grundwassergüte Kalium
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','water_kalium','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_6kal?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_6kal&outputFormat=application/json','restrictive');

-- Grundwassergüte Ortho-Phosphat
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','water_ortho_phosphat','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_7oph?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_7oph&outputFormat=application/json','restrictive');

-- Grundwassergüte Sulfat
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','water_sulfat','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_3sul?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_3sul&outputFormat=application/json','restrictive');

-- Grundwassermessstellen
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('point','groundwater_measuring_points','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sp_gw_messstellen?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sp_gw_messstellen&outputFormat=application/json','restrictive');


--Grundwassertemperatur - Durchschnittstemperatur des Grundwasser 20 - 100 m
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('multipolygon','mean_water_temp_20to100','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s02_14_06gwdtemp_20_100m_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s02_14_06gwdtemp_20_100m_2020&outputFormat=application/json','efficiency');

--Grundwassertemperatur 20 m unter Geländeoberfläche
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('multipolygon','mean_water_temp_20','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s02_14_01gwtemp_20m_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s02_14_01gwtemp_20m_2020&outputFormat=application/json','efficiency');

--Grundwassertemperatur 40 m unter Geländeoberfläche
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('multipolygon','mean_water_temp_40','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s02_14_02gwtemp_40m_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s02_14_02gwtemp_40m_2020&outputFormat=application/json','efficiency');

--Grundwassertemperatur 60 m unter Geländeoberfläche
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('multipolygon','mean_water_temp_60','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s02_14_03gwtemp_60m_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s02_14_03gwtemp_60m_2020&outputFormat=application/json','efficiency');


--Wasserschutzgebiete
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('multipolygon','water_protec_areas','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wsg?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wsg&outputFormat=application/json','restrictive');


--Zu erwartender höchster Grundwasserstand (zeHGW)
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polyline','expe_max_groundwater_hight','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/wfs02_19zeHGW2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:wfs02_19zeHGW2015&outputFormat=application/json','restrictive');

-- Gebäudeflaechen
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','building_surfaces','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_alkis_gebaeudeflaechen?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_alkis_gebaeudeflaechen&outputFormat=application/json','restrictive');