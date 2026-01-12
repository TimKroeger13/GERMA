using System.Globalization;
using System.Text.RegularExpressions;
using System.Transactions;
using GERMAG.DataModel.Database;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

public interface IGetTemplateBHE
{
    Task<TemplateBHE?> CreateTemplateBHE(LandParcel landParcelElement, RequestContext request_context, EnergyMapDemandData energyMapDemandData);
}

public class GetTemplateBHE(DataContext context, IParameterDeserialator parameterDeserialator, IThermalConductivity thermalConductivity, IExtractionCalculation extractionCaclculation) : IGetTemplateBHE
{

    public async Task<TemplateBHE?> CreateTemplateBHE(LandParcel landParcelElement, RequestContext request_context, EnergyMapDemandData energyMapDemandData)
    {
        await using var transaction = context.Database.BeginTransaction();

        var bufferedGeometry = landParcelElement.Geometry?.Buffer(-request_context.LandParcelDistance ?? OfficalParameters.LandParcelDistance);

        var IntersectingGeometry = context.GeoData
            .Where(gd => gd.ParameterKey != landParcelElement.ParameterKey && gd.Geom!.Intersects(bufferedGeometry))
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

        //Get Elements for area

        //Water Protection area

        var WaterProcText = intersectingResult.Where(element => element.Type == TypeOfData.water_protec_areas).ToList();

        /*
        foreach (var WaterProcElement in WaterProcText)
        {
            if (WaterProcElement != null)
            {
                var DeserializedWaterProcText = await Task.Run(() => parameterDeserialator.DeserializeParameters(WaterProcElement?.Parameter ?? ""));
                var IsWaterProcBool = protectionRex.IsMatch(DeserializedWaterProcText.Verordnung ?? string.Empty);
                if (IsWaterProcBool == true) { return null; } //Break condition for water protection areas
            }
        }*/

        //Depth Values

        var UnserilizedDepthRestrictions = intersectingResult.Where(element => element.Type == TypeOfData.geologic_sections_berlin).ToList();
        var UnserilizedHolsteinRestrictionZone = intersectingResult.Where(element => element.Type == TypeOfData.holstein_restrictions).ToList();
        double? MaxDepth;
        double? LimitedDepthFactorMax;
        double? MinimalMaxDepth = Double.PositiveInfinity;

        foreach (var UDR_element in UnserilizedDepthRestrictions)
        {

            var DeserializedDepthRestrictions = await Task.Run(() => parameterDeserialator.DeserializeParameters(UDR_element?.Parameter ?? ""));
            var MaxDepth_rup = Math.Abs(DeserializedDepthRestrictions.Rupel_gok ?? 0);

            if (UnserilizedHolsteinRestrictionZone.Count == 0)
            {

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
                var MaxDepth_holstein = (byte)Math.Abs(DeserializedDepthRestrictions.Holstein_gok ?? 0);
                MaxDepth = Math.Min(MaxDepth_holstein, MaxDepth_rup);
            }        
            
            if (MaxDepth < MinimalMaxDepth)
            {
                MinimalMaxDepth = MaxDepth;
            }
        }

        //Apdet Minimal Maxdepth

        if (MinimalMaxDepth > 100)
        {
            MinimalMaxDepth -= MinimalMaxDepth % OfficalParameters.DepthChunkValue;
        }
        else
        {
            MinimalMaxDepth -= MinimalMaxDepth % OfficalParameters.ShallowChunkValue;
        }

        //Apdet User value
        MinimalMaxDepth = Math.Min((double)MinimalMaxDepth, (double)request_context.MaximalDrillingDepth!);


        LimitedDepthFactorMax = Math.Min(MinimalMaxDepth.GetValueOrDefault(), OfficalParameters.LimitedDepthFactorMax);


        //Potential Values
        List<int> PotentialDepth = new() { 100, 80, 60, 40, 0 }; //Berlin spesific

        int GeoPotenDepth = PotentialDepth.Find(value => value <= MinimalMaxDepth);

        double? GeoPoten = null;
        double? ThermalCon = null;
        double? Temperature = null;

        if (GeoPotenDepth >= 100)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha, "La_100txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_100, "La_100txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_20to100, "Grwtemp_text");
        }
        else if (GeoPotenDepth >= 80)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_80m_with_2400ha, "La_80txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_80, "La_80txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_20to100, "Grwtemp_text");
        }
        else if (GeoPotenDepth >= 60)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_60m_with_2400ha, "La_60txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_60, "La_60txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_60, "Grwtemp_text");
        }
        else
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_40m_with_2400ha, "La_40txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_40, "La_40txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_40, "Grwtemp_text");
        }

        //error correction
        if (GeoPoten < 0) { GeoPoten = 0; }
        if (ThermalCon <0) { ThermalCon = 0; }
        if (Temperature<0) { Temperature= 0; }

        //Find Fitting Amount of Probes - Optimal amount when it possible to drill them
        //Calculate Extraction

        var FlowIsTurbolent = request_context.FlowIsTurbolent;
        var Regeneration = request_context.Regeneration;

        double RawExtraion;
        double TotalExtration = 0;
        int BHECount = 0;

        var EnegeryTargetvalue = energyMapDemandData.ActInsolation;

        var AdaptedConductivity = thermalConductivity.GetThermalConductivity(ThermalCon, MinimalMaxDepth);

        while (TotalExtration < EnegeryTargetvalue)
        {
            BHECount++;

            RawExtraion = extractionCaclculation.GetExtraction(AdaptedConductivity, BHECount, FlowIsTurbolent, Regeneration ?? GERMAG.Shared.Regeneration.None) * MinimalMaxDepth * OfficalParameters.VollLoadhours / 1000 * BHECount ?? throw new Exception("CE: Regeneration could not be calculated");

            TotalExtration = RawExtraion / ((request_context.Cop ?? OfficalParameters.FixCOP) - 1) + RawExtraion;

        }

        if (BHECount == 0)
        {
            BHECount = int.MaxValue;
        }

        var BHE_Template_Element = new TemplateBHE
        {
            MinimalMaxdepth = MinimalMaxDepth,
            BHECount = BHECount,
            GeoPotenDepth = GeoPotenDepth,
            AdaptedConductivity = AdaptedConductivity,
            ThermalCon = ThermalCon
        };

        return BHE_Template_Element;

    }



    private double? GetValue(List<GeometryElementParameter> ProbeSpesificIntersectingResults, TypeOfData typeOfData, String ParameterName)
    {
        var UnserilizedParameter = ProbeSpesificIntersectingResults.Where(element => element.Type == typeOfData).ToList();

        if (UnserilizedParameter == null) { return null; }

        var propertyInfo = typeof(GERMAG.Shared.Properties).GetProperty(ParameterName);
        if (propertyInfo == null) { return null; }

        double? HeighestValue = double.NegativeInfinity;

        foreach (var SingleUnParameter in UnserilizedParameter)
        {
            GERMAG.Shared.Properties DeserializedGeoPoten = parameterDeserialator.DeserializeParameters(SingleUnParameter?.Parameter ?? "");

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
    private Regex protectionRex = new Regex("schutz", RegexOptions.IgnoreCase);
}
