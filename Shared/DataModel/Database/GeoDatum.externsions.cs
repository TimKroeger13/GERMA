using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class GeoDatum
{
    public GeoDatum(GeoDatum model)
    {
        ParameterKey = model.ParameterKey;
        Geom = model.Geom;
    }

    public GeoDatum Model()
    {
        return new()
        {
            ParameterKey = ParameterKey,
            Geom = Geom,
            Id = Id
        };
    }
}
