using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class FindUsageArea
{
    public FindUsageArea()
    {
    }

    public string? Column { get; set; }
    public Geometry? Geom { get; set; }
}
