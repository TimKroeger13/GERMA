using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class Restricion
{
    public NetTopologySuite.Geometries.Geometry? Geometry_Usable { get; set; }
    public NetTopologySuite.Geometries.Geometry? Geometry_Restiction { get; set; }
    public String? Geometry_Usable_geoJson { get; set; }
    public String? Geometry_Restiction_geoJson { get; set; }
    public double? Usable_Area { get; set; }
    public double? Restiction_Area { get; set; }
}
