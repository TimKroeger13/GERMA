namespace GERMAG.DataModel.Database;

public enum TypeOfData
{
    land_parcels,
    dgm,
    geo_poten_restrict,
    veg_height,
    main_water_lines,
    groundwater_surface_distance,
    ground_water_height_main,
    ground_water_height_tension,
    water_ammonium,
    water_bor,
    water_chlor,
    water_kalium,
    water_sulfat,
    water_ortho_phosphat,
    electrical_con,
    mean_water_temp_20to100,
    mean_water_temp_20,
    mean_water_temp_40,
    mean_water_temp_60,
    mean_water_temp_80,
    depth_restrictions_rup,
    geodrilling_data,
    geological_sections,
    geo_drawing,
    water_protec_areas,
    expe_max_groundwater_hight,
    geo_poten_100m_with_2400ha,
    geo_poten_100m_with_1800ha,
    geo_poten_80m_with_2400ha,
    geo_poten_80m_with_1800ha,
    geo_poten_60m_with_2400ha,
    geo_poten_60m_with_1800ha,
    geo_poten_40m_with_2400ha,
    geo_poten_40m_with_1800ha,
    thermal_con_40,
    thermal_con_60,
    thermal_con_80,
    thermal_con_100,
    groundwater_measuring_points,
    building_surfaces,
    depth_restrictions,
    area_usage,
    tree_points,
    tree_vector,
    holstein_restrictions,
    geologic_sections_berlin,
    protection_area
}
public enum Area
{
    berlin
}
public enum Range
{
    near_range,
    far_range
}
public enum Service
{
    restrictive,
    efficiency
}
public enum JsonFormat
{
    single_coordiantes,
    normal,
    short_coordiantes,
    long_coordiantes
}
public enum Geometry_Type
{
    point,
    polygon,
    polyline,
    raster,
    multipolygon,
    empty
}

public enum FingerPrintTypes
{
    sessionLoaded,
    sessionEnded,
    initialRequest,
    reportRequest
}