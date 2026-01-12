namespace GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;

public partial class GeoDatum
{
    public GeoDatum()
    {
    }

    public int Id { get; set; }
    public int ParameterKey { get; set; }
    public Geometry? Geom { get; set; }
    public string? Parameter { get; set; }
    public virtual GeothermalParameter ParameterKeyNavigation { get; set; } = null!;
}
