-- Consolidated PostGIS Database Setup for Geothermal Data
-- This file consolidates all changes from Version 2

-- ============================================================================
-- SECTION 1: Initial Database Setup
-- ============================================================================
-- Note: Run this section in the 'postgres' database first
-- CREATE EXTENSION postgis_raster;
-- CREATE DATABASE germa TEMPLATE postgres;

-- After creating the database, connect to 'germa' and run the rest below

-- ============================================================================
-- SECTION 2: Create ENUMs and Tables
-- ============================================================================

CREATE TYPE typeofdata AS ENUM (
    'land_parcels','dgm','geo_poten_restrict','veg_height',
    'main_water_lines','groundwater_surface_distance','ground_water_height_main',
    'ground_water_height_tension','water_ammonium','water_bor','water_chlor',
    'water_kalium','water_sulfat','water_ortho_phosphat','electrical_con',
    'mean_water_temp_20to100','mean_water_temp_20','mean_water_temp_40',
    'mean_water_temp_60','geodrilling_data','geological_sections','geo_drawing',
    'water_protec_areas','expe_max_groundwater_hight','geo_poten_100m_with_2400ha',
    'geo_poten_100m_with_1800ha','geo_poten_80m_with_2400ha','geo_poten_80m_with_1800ha',
    'geo_poten_60m_with_2400ha','geo_poten_60m_with_1800ha','geo_poten_40m_with_2400ha',
    'geo_poten_40m_with_1800ha','thermal_con_40','thermal_con_60','thermal_con_80',
    'thermal_con_100','groundwater_measuring_points','building_surfaces',
    'mean_water_temp_80','tree_vector','holstein_restrictions',
    'geologic_sections_berlin','protection_area','area_usage'
);

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

CREATE TABLE geo_data (
    id SERIAL PRIMARY KEY NOT NULL,
    parameter_key integer NOT NULL,
    geom geometry,
    parameter json,
    CONSTRAINT parameter_key FOREIGN KEY (parameter_key)
        REFERENCES public.geothermal_parameter (id)
);

-- ============================================================================
-- SECTION 3: Insert Initial Data with Final URLs
-- ============================================================================

-- Land Parcels
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','land_parcels','berlin','near_range','https://gdi.berlin.de/services/wfs/alkis_flurstuecke?service=wfs&version=2.0.0&request=GetFeature&typeNames=alkis_flurstuecke:flurstuecke&outputFormat=application/json','restrictive');

-- Geothermal Potential - Extraction Power (Entzugsleistung)
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_40m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_40&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_40m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_40&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_60m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_60&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_60m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_60&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_80m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_80&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_80m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_80&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_100m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_100&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','geo_poten_100m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_100&outputFormat=application/json','efficiency');

-- Thermal Conductivity (Wärmeleitfähigkeit)
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','thermal_con_40','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_40&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','thermal_con_60','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_60&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','thermal_con_80','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_80&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','thermal_con_100','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_100&outputFormat=application/json','efficiency');

-- Restriction Areas
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('multipolygon','geo_poten_restrict','berlin','near_range','restrictionZone.geojson','restrictive');

-- Groundwater Surface Distance
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('multipolygon','groundwater_surface_distance','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_flurabstand_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_flurabstand_2020:a_panketal&outputFormat=application/json','efficiency');

-- Groundwater Height Lines
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polyline','ground_water_height_main','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_grundwassergl_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassergl_2020:bb_hgwl_gwms_li&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polyline','ground_water_height_tension','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_gwgleichen_panke?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_gwgleichen_panke&outputFormat=application/json','efficiency');

-- Groundwater Quality Parameters
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','water_ammonium','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_4amm?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_4amm&outputFormat=application/json','restrictive');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','water_bor','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_8bor?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_8bor&outputFormat=application/json','restrictive');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','water_chlor','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_2chlo?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_2chlo&outputFormat=application/json','restrictive');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','electrical_con','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_1leit?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_1leit&outputFormat=application/json','restrictive');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','water_kalium','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_6kal?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_6kal&outputFormat=application/json','restrictive');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','water_ortho_phosphat','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_7oph?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_7oph&outputFormat=application/json','restrictive');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','water_sulfat','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/sw02_04_3sul?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:sw02_04_3sul&outputFormat=application/json','restrictive');

-- Groundwater Measuring Points
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('point','groundwater_measuring_points','berlin','near_range','groundwater_measuring_points.geojson','restrictive');

-- Groundwater Temperature
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('multipolygon','mean_water_temp_20to100','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:ec_gwtemp100_2015_fl&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('multipolygon','mean_water_temp_20','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:ac_gwtemp20_2015_fl&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('multipolygon','mean_water_temp_40','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:bc_gwtemp40_2015_fl&outputFormat=application/json','efficiency');

INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('multipolygon','mean_water_temp_60','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:cc_gwtemp60_2015_fl&outputFormat=application/json','efficiency');

-- Groundwater Temperature 80m
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('mean_water_temp_80','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_grundwassertemperatur_2015?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_grundwassertemperatur_2015:dc_gwtemp80_2015_fl&outputFormat=application/json','efficiency','multipolygon');

-- Water Protection Areas
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('multipolygon','water_protec_areas','berlin','near_range','https://gdi.berlin.de/services/wfs/wsg?service=wfs&version=2.0.0&request=GetFeature&typeNames=wsg:wsg&outputFormat=application/json','restrictive');

-- Expected Maximum Groundwater Height
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polyline','expe_max_groundwater_hight','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_zehgw?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_zehgw:ca_zehgw_linien&outputFormat=application/json','restrictive');

-- Building Surfaces
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
VALUES ('polygon','building_surfaces','berlin','near_range','https://gdi.berlin.de/services/wfs/alkis_gebaeude?service=wfs&version=2.0.0&request=GetFeature&typeNames=alkis_gebaeude:gebaeude&outputFormat=application/json','restrictive');

-- Tree Vector Data
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree1.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree2.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree3_1_1.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree3_1_2.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree3_1_3.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree3_2.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree3_3.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree4.geojson','restrictive','multipolygon');

INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('tree_vector','berlin','near_range','tree5.geojson','restrictive','multipolygon');

-- Holstein Restrictions
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('holstein_restrictions','berlin','near_range','holstein_restrictions.geojson','restrictive','multipolygon');

-- Geologic Sections Berlin
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('geologic_sections_berlin','berlin','near_range','geologic_sections_berlin.geojson','efficiency','multipolygon');

-- Geodrilling Data
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('geodrilling_data','berlin','near_range','drilling_points.geojson','restrictive','point');

-- Protection Areas
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service, geometry_type)
VALUES ('protection_area','berlin','near_range','schutzgebiete.geojson','restrictive','multipolygon');

--depth restrictions Berlin (Holstien)
INSERT INTO geothermal_parameter (geometry_type,typeofdata, area, range, getrequest, service)
    VALUES ('polygon','area_usage','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_alkis_tatsaechlichenutzungflaechen?REQUEST=GetCapabilities&SERVICE=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_alkis_tatsaechlichenutzungflaechen&outputFormat=application/json','restrictive');
    
