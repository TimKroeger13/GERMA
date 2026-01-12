using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class RequestContext
{
    public double? CustomProbeDistance { get; set; } = OfficalParameters.ProbeDistance;
    public bool? UseBuildings { get; set; } = true;
    public double? LandParcelDistance { get; set; } = OfficalParameters.LandParcelDistance;
    public bool? UseTrees { get; set; } = true;
    public double? TreeBuffer { get; set; } = 0;
    public int? MinimalBhePerInduvidualArea { get; set; } = 0;
    public int? EnergyDemand { get; set; } = null;
    public bool? FlowIsTurbolent { get; set; } = true;
    public Regeneration? Regeneration { get; set; } = null;
    public bool? CalculateMaximalFieldSize { get; set; } = false;
    public int? MaximalDrillingDepth { get; set; } = (int)OfficalParameters.DepthFactorMax;
    public double? Cop { get; set; } = OfficalParameters.FixCOP;
    public string? GeojsonMinus { get; set; } = null;
    public string? GeojsonSuperior { get; set; } = null;
    public int? Srid { get; set; } = null;
    
}