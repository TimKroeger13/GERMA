using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class GeometryElementParameter
{
    public TypeOfData? Type { get; set; }
    public int? ParameterKey { get; set; }
    public string? Parameter { get; set; }
    public Properties? JsonDataParameter { get; set; }
    public NetTopologySuite.Geometries.Geometry? Geometry {set; get;}
}
