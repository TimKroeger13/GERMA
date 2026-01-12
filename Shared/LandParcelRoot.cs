using NetTopologySuite.Geometries;
using GERMAG.DataModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class LandParcel
{
    public int? GeoDataID { get; set; }
    public int? ParameterKey { get; set; }
    public string? Parameter { get; set; }
    public NetTopologySuite.Geometries.Geometry? Geometry { get; set; }
    public string? GeometryJson { get; set; }
    public bool? Error { get; set; }
    public string? LandParcels { get; set; }
}
