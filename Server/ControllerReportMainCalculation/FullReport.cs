using GERMAG.DataModel.Database;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Shared;

public interface IFullreport
{
    Task<IEnumerable<Report>> CalcualteFullReport(List<double> Xcor, List<double> Ycor, int Srid, double ProbeDistance, bool UseBuildings, bool UseTrees, double LandParcelDistance, double TreeBuffer, int MinimalBhePerInduvidualArea, int EnergyDemand, bool FlowIsTurbolent, Regeneration Regeneration, bool CalculateMaximalFieldSize, int MaximalDrillingDepth, string? userId, double? cop);
}

public class Fullreport(IReceiveLandParcel receiveLandParcel, ICreateFingerPrint createFingerPrint, ICoreReport coreReport) : IFullreport
{
    public async Task<IEnumerable<Report>> CalcualteFullReport(
    List<double> Xcor,
    List<double> Ycor,
    int Srid,
    double ProbeDistance,
    bool UseBuildings,
    bool UseTrees,
    double LandParcelDistance,
    double TreeBuffer,
    int MinimalBhePerInduvidualArea,
    int EnergyDemand,
    bool FlowIsTurbolent,
    Regeneration Regeneration,
    bool CalculateMaximalFieldSize,
    int MaximalDrillingDepth,
    string? userId,
    double? cop)
    {
        if (ProbeDistance == 0) { ProbeDistance = OfficalParameters.ProbeDistance; }
        if (MaximalDrillingDepth == 0) { MaximalDrillingDepth = (int)OfficalParameters.DepthFactorMax; }
        if (cop < 2 || cop == null) { cop = OfficalParameters.FixCOP; }
        if (LandParcelDistance <= 0) { LandParcelDistance = 0.000001; }

        RequestContext request_context = new()
        {
            CustomProbeDistance = ProbeDistance + 0.05,
            UseBuildings = UseBuildings,
            LandParcelDistance = LandParcelDistance,
            UseTrees = UseTrees,
            TreeBuffer = TreeBuffer,
            MinimalBhePerInduvidualArea = MinimalBhePerInduvidualArea,
            EnergyDemand = EnergyDemand,
            FlowIsTurbolent = FlowIsTurbolent,
            Regeneration = Regeneration,
            CalculateMaximalFieldSize = CalculateMaximalFieldSize,
            MaximalDrillingDepth = MaximalDrillingDepth,
            Cop = cop
        };

        LandParcel landParcelElement = await receiveLandParcel.GetLandParcel(Xcor, Ycor, Srid);

        var FinalReport = await coreReport.MainReportCalculation(landParcelElement, request_context, true);

        await createFingerPrint.CreateUniqueFingerPrint(userId ?? "", FingerPrintTypes.reportRequest, landParcelElement.Geometry);

        return FinalReport;
    }
}