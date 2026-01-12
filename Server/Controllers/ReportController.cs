using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using Microsoft.AspNetCore.Cors;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using GERMAG.Server.ReportCreation;
using GERMAG.Server.GeometryCalculations;
using NetTopologySuite.IO;
using GERMAG.Shared.PointProperties;
using NetTopologySuite.Geometries;
using System.ComponentModel.Design;
using NetTopologySuite.Operation.Overlay;
using GERMAG.DataModel.Database;
using Microsoft.AspNetCore.RateLimiting;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(IShortReport shortReport, IFullreport fullreport, ICustomReport customReport) : ControllerBase // IGetPolylineData getPolylineData)
{
    [HttpGet("reportdata")]
    [EnableCors(CorsPolicies.GetAllowed)]
    [EnableRateLimiting("fixedShortReport")]
    public async Task<IEnumerable<Report>> GetReport([FromQuery] List<double> Xcor, [FromQuery] List<double> Ycor, [FromQuery] int Srid, [FromQuery] string? userId)
    {
        var reportList = await shortReport.CalculateShortReport(Xcor, Ycor, Srid, userId);

        return reportList;
    }

    [HttpGet("fullreport")]
    [EnableCors(CorsPolicies.GetAllowed)]
    [EnableRateLimiting("fixedFullReport")]
    public async Task<IEnumerable<Report>> GetFullReport([FromQuery] List<double> Xcor,
    [FromQuery] List<double> Ycor,
    [FromQuery] int Srid,
    [FromQuery] double ProbeDistance,
    [FromQuery] bool UseBuildings,
    [FromQuery] bool UseTrees,
    [FromQuery] double LandParcelDistance,
    [FromQuery] double TreeBuffer,
    [FromQuery] int MinimalBhePerInduvidualArea,
    [FromQuery] int EnergyDemand,
    [FromQuery] bool FlowIsTurbolent,
    [FromQuery] Regeneration Regeneration,
    [FromQuery] bool CalculateMaximalFieldSize,
    [FromQuery] int MaximalDrillingDepth,
    [FromQuery] string? userId,
    [FromQuery] double? cop)
    {
        DateTime start = DateTime.Now;
        Console.WriteLine("Detailed report request on X= " + Xcor[0] + " / Y= " + Ycor[0]);

        var FinalReport = await fullreport.CalcualteFullReport(Xcor, Ycor, Srid, ProbeDistance, UseBuildings, UseTrees, LandParcelDistance, TreeBuffer, MinimalBhePerInduvidualArea, EnergyDemand, FlowIsTurbolent, Regeneration, CalculateMaximalFieldSize, MaximalDrillingDepth, userId, cop);

        TimeSpan total = DateTime.Now - start;
        Console.WriteLine(total);

        return FinalReport;
    }

    [HttpPost("geojsonreport")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IEnumerable<Report>> GetGeoJsonReport([FromBody] GeoJsonRequest request)
    {
        DateTime start = DateTime.Now;
        Console.WriteLine("Geojson Request");

        var FinalReport = await customReport.CalcualteCustomReport(request);

        TimeSpan total = DateTime.Now - start;
        Console.WriteLine(total);

        return FinalReport;
    }
}