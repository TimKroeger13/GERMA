namespace GERMAG.DataModel.Database;

public partial class RequestLessThanOneDay
{
    public RequestLessThanOneDay()
    {
    }

    public string? Userid { get; set; }
    public NetTopologySuite.Geometries.Geometry? StTransform { get; set; }
}
