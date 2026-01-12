using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class AxTree
{
    public AxTree()
    {
    }

    public Geometry? Geom { get; set; }
}
