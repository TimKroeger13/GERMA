using GERMAG.Shared.PointProperties;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using NetTopologySuite.Geometries.Prepared;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetProbeSepcificDataSingleProbe
{
    Task<ProbePoint?> GetSingleProbeData(ProbePoint? SingleProbePoint, List<GeometryElementParameter> intersectingResult, int? NumberOfProbes, RequestContext request_context);
}

public class GetProbeSepcificDataSingleProbe(IParameterDeserialator parameterDeserialator, IRating rating, IThermalConductivity thermalConductivity, IExtractionCalculation extractionCaclculation) : IGetProbeSepcificDataSingleProbe
{
    private Regex protectionRex = new Regex("schutz", RegexOptions.IgnoreCase);
    public async Task<ProbePoint?> GetSingleProbeData(ProbePoint? SingleProbePoint, List<GeometryElementParameter> intersectingResult, int? NumberOfProbes, RequestContext request_context)
    {
        await Task.Delay(1);

        if (SingleProbePoint == null || SingleProbePoint.Geometry == null)
        {
            return null;
        }

        SingleProbePoint.Geometry.SRID = 25833;

        IPreparedGeometry preparedPoint = PreparedGeometryFactory.Prepare(SingleProbePoint.Geometry);

        var ProbeSpesificIntersectingResults = intersectingResult
            .Where(ir => ir.Geometry != null && preparedPoint.Intersects(ir.Geometry))
            .ToList();

        //SingleProbePoint.Geometry
        //Calculate Probe point intersections

        var RestrictionText = ProbeSpesificIntersectingResults.Find(element => element.Type == TypeOfData.geo_poten_restrict);

        bool IsProtectedBool = false;
        if (RestrictionText != null)
        {
            var DeserializedRestrictionText = await Task.Run(() => parameterDeserialator.DeserializeParameters(RestrictionText?.Parameter ?? ""));
            IsProtectedBool = protectionRex.IsMatch(DeserializedRestrictionText.Text ?? string.Empty);
        }

        //Check for water protection
        var WaterProcText = ProbeSpesificIntersectingResults.Find(element => element.Type == TypeOfData.water_protec_areas);

        if(WaterProcText != null){
            var DeserializedWaterProcText = await Task.Run(() => parameterDeserialator.DeserializeParameters(WaterProcText?.Parameter ?? ""));
            var IsWaterProcBool = protectionRex.IsMatch(DeserializedWaterProcText.Verordnung ?? string.Empty);

            if(IsWaterProcBool){
                return null;
            }

        }


        var UnserilizedDepthRestrictions = ProbeSpesificIntersectingResults.Find(element => element.Type == TypeOfData.geologic_sections_berlin);
        //var UnserilizedDepthRestrictions_rup = ProbeSpesificIntersectingResults.Find(element => element.Type == TypeOfData.depth_restrictions_rup);
        var UnserilizedHolsteinRestrictionZone = ProbeSpesificIntersectingResults.Find(element => element.Type == TypeOfData.holstein_restrictions);
        double? MaxDepth;
        double? LimitedDepthFactorMax; 

        if (UnserilizedHolsteinRestrictionZone == null)
        {

            var DeserializedDepthRestrictions = await Task.Run(() => parameterDeserialator.DeserializeParameters(UnserilizedDepthRestrictions?.Parameter ?? ""));
            var MaxDepth_rup = Math.Abs(DeserializedDepthRestrictions.Rupel_gok ?? 0);

            if (MaxDepth_rup < OfficalParameters.DepthFactorMax)
            {
                MaxDepth = MaxDepth_rup;
            }
            else
            {
                MaxDepth = OfficalParameters.DepthFactorMax; //Berlin spesific
            }
        }
        else
        {
            var DeserializedDepthRestrictions = await Task.Run(() => parameterDeserialator.DeserializeParameters(UnserilizedDepthRestrictions?.Parameter ?? ""));
            //var DeserializedDepthRestrictions_rup = await Task.Run(() => parameterDeserialator.DeserializeParameters(UnserilizedDepthRestrictions_rup?.Parameter ?? ""));

            var MaxDepth_rup = Math.Abs(DeserializedDepthRestrictions.Rupel_gok ?? 0);

            var MaxDepth_holstein = (byte)Math.Abs(DeserializedDepthRestrictions.Holstein_gok ?? 0);
            MaxDepth = Math.Min(MaxDepth_holstein, MaxDepth_rup);
        }

        LimitedDepthFactorMax = Math.Min(MaxDepth.GetValueOrDefault(), OfficalParameters.LimitedDepthFactorMax);

        //Poetential = 100,80,60,40
        List<int> PotentialDepth = new() { 100, 80, 60, 40, 0 }; //Berlin spesific

        int GeoPotenDepth = PotentialDepth.Find(value => value <= MaxDepth);

        double? GeoPoten = null;
        double? ThermalCon = null;
        double? Temperature = null;

        if (GeoPotenDepth >= 100)
        {
            GeoPoten = GetValue(ProbeSpesificIntersectingResults, TypeOfData.geo_poten_100m_with_2400ha, "La_100txt");
            ThermalCon = GetValue(ProbeSpesificIntersectingResults, TypeOfData.thermal_con_100, "La_100txt");
            Temperature = GetValue(ProbeSpesificIntersectingResults, TypeOfData.mean_water_temp_20to100, "Grwtemp_text");

            //Adeption for max depth

            if (MaxDepth > 100)
            {
                //Placeholder for the time when the custom calculation Formula for extration exsist
            }
        }
        else if (GeoPotenDepth >= 80)
        {
            GeoPoten = GetValue(ProbeSpesificIntersectingResults, TypeOfData.geo_poten_80m_with_2400ha, "La_80txt");
            ThermalCon = GetValue(ProbeSpesificIntersectingResults, TypeOfData.thermal_con_80, "La_80txt");
            Temperature = GetValue(ProbeSpesificIntersectingResults, TypeOfData.mean_water_temp_20to100, "Grwtemp_text");
        }
        else if (GeoPotenDepth >= 60)
        {
            GeoPoten = GetValue(ProbeSpesificIntersectingResults, TypeOfData.geo_poten_60m_with_2400ha, "La_60txt");
            ThermalCon = GetValue(ProbeSpesificIntersectingResults, TypeOfData.thermal_con_60, "La_60txt");
            Temperature = GetValue(ProbeSpesificIntersectingResults, TypeOfData.mean_water_temp_60, "Grwtemp_text");
        }
        else
        {
            GeoPoten = GetValue(ProbeSpesificIntersectingResults, TypeOfData.geo_poten_40m_with_2400ha, "La_40txt");
            ThermalCon = GetValue(ProbeSpesificIntersectingResults, TypeOfData.thermal_con_40, "La_40txt");
            Temperature = GetValue(ProbeSpesificIntersectingResults, TypeOfData.mean_water_temp_40, "Grwtemp_text");
        }

        //error correction
        if (GeoPoten < 0) { GeoPoten = 0; }
        if(ThermalCon <0){ ThermalCon = 0; }
        if(Temperature<0){ Temperature= 0; }



        if (SingleProbePoint.Properties == null)
        {
            var CorrectedProbpoint = new ProbePoint
            {
                Geometry = SingleProbePoint.Geometry,
                Properties = new Shared.PointProperties.Properties { GeoPoten = null, MaxDepth = null, GeoPotenDepth = null }
            };

            SingleProbePoint = CorrectedProbpoint;
        }

        var ratingFactor = await rating.CalculateRating(MaxDepth, ThermalCon, Temperature, IsProtectedBool);


        //here

        //Calculate temporal thermal conductivity

        var AdaptedConductivity = thermalConductivity.GetThermalConductivity(ThermalCon, MaxDepth);

        //Calculate Extraction

        var FlowIsTurbolent = request_context.FlowIsTurbolent;
        var Regeneration = request_context.Regeneration;

        var EWs_extraction = extractionCaclculation.GetExtraction(AdaptedConductivity, NumberOfProbes, FlowIsTurbolent, Regeneration ?? GERMAG.Shared.Regeneration.None);

        SingleProbePoint.Properties.MaxDepth = MaxDepth;
        SingleProbePoint.Properties.GeoPotenDepth = GeoPotenDepth;
        SingleProbePoint.Properties.GeoPoten = EWs_extraction;
        SingleProbePoint.Properties.Rating = ratingFactor;
        SingleProbePoint.Properties.Crossinfluence_Factor = 0;

        SingleProbePoint.Properties.Extraction_KW_2400_Full_Load_Hours = EWs_extraction * MaxDepth * OfficalParameters.VollLoadhours / 1000;
        SingleProbePoint.Properties.Extraction_KW = EWs_extraction * MaxDepth / 1000;
        SingleProbePoint.Properties.Limited_100m_Extraction_KW_2400_Full_Load_Hours = EWs_extraction * LimitedDepthFactorMax * OfficalParameters.VollLoadhours / 1000;
        SingleProbePoint.Properties.Limited_100m_Extraction_KW = EWs_extraction * LimitedDepthFactorMax / 1000;

        return SingleProbePoint;
    }

    private double? GetValue(List<GeometryElementParameter> ProbeSpesificIntersectingResults, TypeOfData typeOfData, String ParameterName)
    {
        var UnserilizedParameter = ProbeSpesificIntersectingResults.Where(element => element.Type == typeOfData).ToList();

        if (UnserilizedParameter == null) { return null; }

        var propertyInfo = typeof(Shared.Properties).GetProperty(ParameterName);
        if (propertyInfo == null) { return null; }

        double? HeighestValue = double.NegativeInfinity;

        foreach (var SingleUnParameter in UnserilizedParameter)
        {
            Shared.Properties DeserializedGeoPoten = parameterDeserialator.DeserializeParameters(SingleUnParameter?.Parameter ?? "");

            string? propertyValue = propertyInfo.GetValue(DeserializedGeoPoten) as string;
            double? CurrentValue = ParseStringToValue(propertyValue ?? string.Empty);

            if (HeighestValue < CurrentValue) { HeighestValue = CurrentValue; }
        }

        return HeighestValue;
    }

    private double? ParseStringToValue(string valueRange)
    {
        // Replace commas with dots
        valueRange = CommaToDotRegex().Replace(valueRange, ".");
        valueRange = CutRegex().Replace(valueRange, "-");
        valueRange = RemoveLessThanRegex().Replace(valueRange, "");
        valueRange = RemoveGreaterThanRegex().Replace(valueRange, "");

        // Split the string by "bis"
        string[] parts = valueRange.Split('-', '>');

        List<double> numbers = new();

        foreach (string part in parts)
        {
            if (double.TryParse(part.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                numbers.Add(number);
            }
        }

        if (numbers.Count == 0)
        {
            return null;
        }

        double minValue = numbers.Min();
        double maxValue = numbers.Max();

        return (minValue + maxValue) / 2.0;
    }

    private static Regex CutRegex() => new Regex("bis", RegexOptions.Compiled);
    private static Regex CommaToDotRegex() => new Regex(",", RegexOptions.Compiled);
    private static Regex RemoveLessThanRegex() => new Regex("<", RegexOptions.Compiled);
    private static Regex RemoveGreaterThanRegex() => new Regex(">", RegexOptions.Compiled);
}
