using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class Research
{
    public Research()
    {
    }

    public Geometry? Geom { get; set; }
}
