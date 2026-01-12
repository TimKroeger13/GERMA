using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class UsageArea
{
    public UsageArea()
    {
    }

    public string? Uuid { get; set; }
    public string? Bezeich { get; set; }
    public Geometry? Geom { get; set; }
}
