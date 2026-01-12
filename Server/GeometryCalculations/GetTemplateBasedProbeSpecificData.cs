using GERMAG.Server.GeometryCalculations;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;

public interface IGetTemplateBasedProbeSpecificData
{
    List<ProbePoint?> GetPointProbeDataAfterTemplate(LandParcel landParcelElement, List<ProbePoint?> probePoints, RequestContext request_context, TemplateBHE templateBHE);
}

public class GetTemplateBasedProbeSpecificData(IExtractionCalculation extractionCaclculation) : IGetTemplateBasedProbeSpecificData
{
    public List<ProbePoint?> GetPointProbeDataAfterTemplate(LandParcel landParcelElement, List<ProbePoint?> probePoints, RequestContext request_context, TemplateBHE templateBHE)  //Task<List<ProbePoint?>>
    {
        List<ProbePoint?> ListOfProbePoints = [];

        var LimitedDepthFactorMax = Math.Min(templateBHE.MinimalMaxdepth.GetValueOrDefault(), OfficalParameters.LimitedDepthFactorMax);

        foreach (var SingleProbePoint in probePoints)
        {
            if (SingleProbePoint == null || SingleProbePoint.Geometry == null)
            {
                continue;
            }

            var CorrectedProbpoint = new ProbePoint
            {
                Geometry = SingleProbePoint.Geometry,
                Properties = new GERMAG.Shared.PointProperties.Properties { GeoPoten = null, MaxDepth = null, GeoPotenDepth = null },
                GeometryJson = SingleProbePoint.GeometryJson
            };


            var FlowIsTurbolent = request_context.FlowIsTurbolent;
            var Regeneration = request_context.Regeneration;

            var RawExtraion = extractionCaclculation.GetExtraction(templateBHE.AdaptedConductivity, probePoints.Count, FlowIsTurbolent, Regeneration ?? GERMAG.Shared.Regeneration.None);


            

            CorrectedProbpoint.Properties.MaxDepth = templateBHE.MinimalMaxdepth;
            CorrectedProbpoint.Properties.GeoPotenDepth = templateBHE.GeoPotenDepth;
            CorrectedProbpoint.Properties.GeoPoten = RawExtraion;
            CorrectedProbpoint.Properties.Rating = 0;
            CorrectedProbpoint.Properties.Crossinfluence_Factor = 0;
            CorrectedProbpoint.Properties.ThermalCon = templateBHE.ThermalCon;

            CorrectedProbpoint.Properties.Extraction_KW_2400_Full_Load_Hours = RawExtraion * templateBHE.MinimalMaxdepth * OfficalParameters.VollLoadhours / 1000;
            CorrectedProbpoint.Properties.Extraction_KW = RawExtraion * templateBHE.MinimalMaxdepth / 1000;
            CorrectedProbpoint.Properties.Limited_100m_Extraction_KW_2400_Full_Load_Hours = RawExtraion * LimitedDepthFactorMax * OfficalParameters.VollLoadhours / 1000;
            CorrectedProbpoint.Properties.Limited_100m_Extraction_KW = RawExtraion * LimitedDepthFactorMax / 1000;
            CorrectedProbpoint.Properties.SubAreaFieldNumber = SingleProbePoint.Properties?.SubAreaFieldNumber;

            ListOfProbePoints.Add(CorrectedProbpoint);
        }

        return ListOfProbePoints;
    }
}