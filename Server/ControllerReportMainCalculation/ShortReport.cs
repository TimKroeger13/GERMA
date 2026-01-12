using GERMAG.DataModel.Database;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

public interface IShortReport
{
    Task<IEnumerable<Report>> CalculateShortReport([FromQuery] List<double> Xcor, [FromQuery] List<double> Ycor, [FromQuery] int Srid, [FromQuery] string? userId);
}

public class ShortReport(ICreateReportAsync createReport, IReceiveLandParcel receiveLandParcel, IRestrictionFromLandParcel restrictionFromLandParcel, ICreateFingerPrint createFingerPrint) : IShortReport
{
    public async Task<IEnumerable<Report>> CalculateShortReport([FromQuery] List<double> Xcor, [FromQuery] List<double> Ycor, [FromQuery] int Srid, [FromQuery] string? userId)
    {
        RequestContext request_context = new RequestContext();

        Console.WriteLine("Area request on X= " + Xcor[0] + " / Y= " + Ycor[0]);

        LandParcel landParcelElement = await receiveLandParcel.GetLandParcel(Xcor, Ycor, Srid);

        if (landParcelElement.Error == true) { return new[] { new Report { Error = "Error 512: Bisher wird nur das Gebiet Berlin von GERMA unterstützt.\nFür Gebiete in Brandenburg kontaktieren sie die O-GT-T!" } }; }
        //if (landParcelElement.Geometry is MultiPolygon) { return new[] { new Report { Error = "Error 514: Die gewählten Flurstücke müssen zusammenhängen!" } }; }

        Restricion RestrictionFile = await restrictionFromLandParcel.CalculateRestrictions(landParcelElement, request_context);

        IEnumerable<Report> polygonBasedReport = await createReport.CreateGeothermalReportAsync(landParcelElement, RestrictionFile, true);

        List<Report> reportList = polygonBasedReport.ToList();
        reportList[0].Geometry = landParcelElement.GeometryJson;
        reportList[0].Land_parcel_number = landParcelElement.LandParcels;

        //User Tracking - Inital request
        await createFingerPrint.CreateUniqueFingerPrint(userId ?? "", FingerPrintTypes.initialRequest, landParcelElement.Geometry);

        return reportList;
    }
}