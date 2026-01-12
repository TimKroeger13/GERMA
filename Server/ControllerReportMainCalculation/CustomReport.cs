using GERMAG.DataModel.Database;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using Microsoft.AspNetCore.Mvc;

public interface ICustomReport
{
    Task<IEnumerable<Report>> CalcualteCustomReport([FromBody] GeoJsonRequest request);
}

public class CustomReport(IGeometryFromGeoJson geometryFromGeoJson, ICreateFingerPrint createFingerPrint, ICoreReport coreReport) : ICustomReport
{
    public async Task<IEnumerable<Report>> CalcualteCustomReport([FromBody] GeoJsonRequest request)
    {
        RequestContext request_context = new RequestContext();

        if (request.ProbeDistance == 0) { request.ProbeDistance = OfficalParameters.ProbeDistance; }
        if (request.MaximalDrillingDepth == 0) { request.MaximalDrillingDepth = (int)OfficalParameters.DepthFactorMax; }
        if (request.Cop < 2) { request.Cop = OfficalParameters.FixCOP; }
        if (request.LandParcelDistance <= 0) { request.LandParcelDistance = 0.000001; }

        request_context.CustomProbeDistance = request.ProbeDistance + 0.05;
        request_context.UseBuildings = request.UseBuildings;
        request_context.LandParcelDistance = request.LandParcelDistance;
        request_context.UseTrees = request.UseTrees;
        request_context.TreeBuffer = request.TreeBuffer;
        request_context.MinimalBhePerInduvidualArea = request.MinimalBhePerInduvidualArea;
        request_context.EnergyDemand = request.EnergyDemand;
        request_context.FlowIsTurbolent = request.FlowIsTurbolent;
        request_context.CalculateMaximalFieldSize = request.CalculateMaximalFieldSize;
        request_context.MaximalDrillingDepth = request.MaximalDrillingDepth;
        request_context.Cop = request.Cop;
        request_context.GeojsonMinus = request.GeojsonMinus;
        request_context.GeojsonSuperior = request.GeojsonSuperior;
        request_context.Srid = request.Srid;

        if (Enum.TryParse<Regeneration>(request.Regeneration, true, out var parsedRegeneration))
        {
            request_context.Regeneration = parsedRegeneration;
        }
        else
        {
            request_context.Regeneration = Regeneration.None;
        }

        if (request == null || request.Geojson == null || request.Srid <= 0)
        { //error
            throw new Exception("Geometry is incorrect");
        }

        LandParcel landParcelElement = await geometryFromGeoJson.GetGeometryFromgeoJson(request.Geojson, request.Srid);

        var FinalReport = await coreReport.MainReportCalculation(landParcelElement, request_context, false);

        await createFingerPrint.CreateUniqueFingerPrint(request.UserId ?? "", FingerPrintTypes.reportRequest, landParcelElement.Geometry);

        return FinalReport;
    }
}