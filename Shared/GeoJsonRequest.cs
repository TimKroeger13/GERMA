using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class GeoJsonRequest
{
    public int Srid { get; set; }
    public string? Geojson { get; set; }
    public string? GeojsonMinus { get; set; }
    public string? GeojsonSuperior { get; set; }
    public double? ProbeDistance { get; set; }
    public bool? UseBuildings { get; set; }
    public double? LandParcelDistance { get; set; }
    public bool? UseTrees { get; set; }
    public double? TreeBuffer { get; set; }
    public int? MinimalBhePerInduvidualArea { get; set; }
    public int? EnergyDemand { get; set; }
    public bool? FlowIsTurbolent { get; set; }
    public string? Regeneration { get; set; }
    public bool? CalculateMaximalFieldSize { get; set; }
    public int? MaximalDrillingDepth { get; set; }
    public string? UserId { get; set; }
    public double? Cop { get; set; }
}
