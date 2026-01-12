using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GERMAG.DataModel;

public class GeoDatum
{
    public int Id { get; set; }
    public int ParameterKey { get; set; }
    public Geometry? Geom { get; set; }
}
