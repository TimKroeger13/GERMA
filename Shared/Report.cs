using GERMAG.Shared.PointProperties;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class Report
{
    public String? Geo_poten_100m_with_2400ha { get; set; }
    public String? Geo_poten_80m_with_2400ha { get; set; }
    public String? Geo_poten_60m_with_2400ha { get; set; }
    public String? Geo_poten_40m_with_2400ha { get; set; }
    public String? Geo_poten_100m_with_1800ha { get; set; }
    public String? Geo_poten_80m_with_1800ha { get; set; }
    public String? Geo_poten_60m_with_1800ha { get; set; }
    public String? Geo_poten_40m_with_1800ha { get; set; }
    public String? Thermal_con_100 { get; set; }
    public String? Thermal_con_80 { get; set; }
    public String? Thermal_con_60 { get; set; }
    public String? Thermal_con_40 { get; set; }
    public String? Mean_water_temp_20to100 { get; set; }
    public String? Mean_water_temp_80 { get; set; }
    public String? Mean_water_temp_60 { get; set; }
    public String? Mean_water_temp_40 { get; set; }
    public String? Mean_water_temp_20 { get; set; }
    public List<String>? Geo_poten_restrict { get; set; }
    public String? Land_parcel_number { get; set; }
    public String? Land_parcels_gemeinde { get; set; }
    public String? Building_begzgkt { get; set; }
    public double? ZeHGW { get; set; }
    public List<String>? Verordnung { get; set; }
    public List<String>? Veror_link { get; set; }
    public String? Geometry { get; set; }
    public String? Error { get; set; }

    public String? Geometry_Usable { get; set; }
    public String? Geometry_Restiction { get; set; }
    public double? Usable_Area { get; set; }
    public double? Restiction_Area { get; set; }

    public List<ProbePoint?>? ProbePoint { get; set; }
    public String? Geometry_LeftOverArea { get; set; }
    public bool? ActiveRestriction { get; set; }

    public double? Crossinfluence_Factor { get; set; }
    public double? Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? Extraction_KW { get; set; }
    public double? Limited_100m_Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? Limited_100m_Extraction_KW { get; set; }
    public String? Holstein { get; set; }
    public String? Rupelton { get; set; }
    public String? TotalMaxDepth { get; set; }
    public int? ActInsolation { get; set; }
    public int? OptimalInsolation { get; set; }
    public double? ActInsolation_Coverage_100 { get; set; }
    public double? OptimalInsolation_Coverage_100 { get; set; }
    public double? ActInsolation_Coverage_Maxdepth { get; set; }
    public double? OptimalInsolation_Coverage_Maxdepth { get; set; }
    public bool? FlowIsTurbolent { get; set; }
    public String? Regeneration { get; set; }

    public double? REG_NO_Limited_100m_Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? REG_NO_Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? REG_HALF_Limited_100m_Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? REG_HALF_Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? REG_FULL_Limited_100m_Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? REG_FULL_Extraction_KW_2400_Full_Load_Hours { get; set; }
    public double? COP { get; set; }
    public List<String>? ProtecitonList { get; set; } 
}