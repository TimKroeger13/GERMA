using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared.PointProperties;

public class ProbePoint
{
    public string Type => "Feature";
    public NetTopologySuite.Geometries.Geometry? Geometry { get; set; }
    public string? GeometryJson { get; set; }
    public Properties? Properties { get; set; }
}

public class Properties
{
    public double? GeoPoten { get; set; } = null;
    public double? MaxDepth { get; set; } = null;
    public double? GeoPotenDepth { get; set; } = null;
    public double? Extraction_KW_2400_Full_Load_Hours { get; set; } = null;
    public double? Extraction_KW { get; set; } = null;
    public double? Limited_100m_Extraction_KW_2400_Full_Load_Hours { get; set; } = null;
    public double? Limited_100m_Extraction_KW { get; set; } = null;
    public double? Crossinfluence_Factor { get; set; } = null;
    public decimal? Rating { get; set; } = null;
    public int? SubAreaFieldNumber { get; set; } = null;
    public double? ThermalCon { get; set; } = null;
}
