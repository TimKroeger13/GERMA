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

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]


public class GeodatastreamController(IReciveBoundingBoxInformation reciveBoundingBoxInformation) : ControllerBase // IGetPolylineData getPolylineData)
{
    [HttpPost("Getvectordatasteam")]
    [EnableCors(CorsPolicies.GetAllowed)]

    public async Task<BoundingBoxData?> StreamVectorData([FromBody] VectorDataStreamRequest request)
    {
        DateTime start = DateTime.Now;

        Console.WriteLine("Calcualting BoundingBox");

        if(request == null){ throw new Exception("CE: Datastream request is null"); }

        (double, double) TL = (request.MinLng, request.MaxLat);
        (double, double) BR = (request.MaxLng, request.MinLat);
        var srid = request.Srid;

        BoundingBoxData? BoundingBoxWithDadta = await reciveBoundingBoxInformation.GetBoundingBoxInformation(TL, BR, srid);

        TimeSpan total = DateTime.Now - start;

        Console.WriteLine(total);

        return BoundingBoxWithDadta ?? null;
    }

}