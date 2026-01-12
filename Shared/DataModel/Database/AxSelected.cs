using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class AxSelected
{
    public AxSelected()
    {
    }

    public Geometry? Geom { get; set; }
}
