using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class AxBuilding
{
    public AxBuilding()
    {
    }

    public Geometry? Geom { get; set; }
}
