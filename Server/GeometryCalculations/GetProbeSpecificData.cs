using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using Newtonsoft.Json.Linq;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetProbeSpecificData
{
    Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints, RequestContext request_context);
}

public class GetProbeSpecificData(DataContext context, IGetProbeSepcificDataSingleProbe getProbeSepcificDataSingleProbe) : IGetProbeSpecificData
{
    public async Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints, RequestContext request_context)
    {

        await using var transaction = context.Database.BeginTransaction();

        var IntersectingGeometry = context.GeoData
            .Where(gd => gd.ParameterKey != landParcelElement.ParameterKey && gd.Geom!.Intersects(landParcelElement.Geometry))
            .Select(gd => new
            {
                gd.ParameterKey,
                gd.Parameter,
                gd.Geom
            });

        List<GeometryElementParameter> intersectingResult = IntersectingGeometry
            .Join(
            context.GeothermalParameter,
            ig => ig.ParameterKey,
            gp => gp.Id,
            (ig, gp) => new GeometryElementParameter
            {
                Type = gp.Type,
                ParameterKey = ig.ParameterKey,
                Parameter = ig.Parameter,
                Geometry = ig.Geom
            }).ToList();

        context.SaveChanges();
        transaction.Commit();

        //probePoints = [probePoints[0]];

        var tasks = probePoints.Select(probePoint =>
            Task.Run(() => getProbeSepcificDataSingleProbe
                .GetSingleProbeData(probePoint, intersectingResult, probePoints.Count, request_context))
        ).ToList();

        var results = await Task.WhenAll(tasks);

        List<ProbePoint?> probePointList = [.. results];

        return probePointList;
    }
}