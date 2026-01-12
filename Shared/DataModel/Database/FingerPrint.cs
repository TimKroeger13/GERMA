namespace GERMAG.DataModel.Database;

public partial class FingerPrint
{
    public FingerPrint()
    {
    }

    public int Id { get; set; }
    public string? Userid { get; set; }
    public NetTopologySuite.Geometries.Geometry? Geom { get; set; }
    public DateTime? Time { get; set; }
}
