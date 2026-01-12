namespace GERMAG.DataModel;

using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

public class GeothermalParameterModel
{
    public int Id { get; set; }

    [Url]
    public string? Getrequest { get; set; }

    [Range(0, int.MaxValue)]
    public int? Srid { get; set; }
    public DateTime? LastUpdate { get; set; }
    public DateTime? LastPing { get; set; }
    public TypeOfData Type { get; set; }
    public Area Area { get; set; }
    public Range Range { get; set; }
    public Service Service { get; set; }
    public Geometry_Type? Geometry_Type { get; set; }
}
