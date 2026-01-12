using GERMAG.Shared.PointProperties;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using NetTopologySuite.Geometries.Prepared;
namespace GERMAG.Server.GeometryCalculations;

public interface IExtractionCalculation
{
    double? GetExtraction(double? Conductivity, int? NumberOfProbes, bool? FlowIsTurbolent, Regeneration Regeneration);
}


//2400 Vollaststunden
//-3 Minimale Rücklauftempertur
//6m Sondenabstand

public class ExtractionCalculation() : IExtractionCalculation
{
    public double? GetExtraction(double? Conductivity, int? NumberOfProbes, bool? FlowIsTurbolent, Regeneration Regeneration)
    {
        if (Conductivity > 4 || Conductivity < 1 || Conductivity == null)
        {
            throw new Exception("CE: Conductivity is not Supportet");
        }

        (double? WM1, double? WM2, double? WM3, double? WM4) ExtractionTable = (null, null, null, null);

        if (Regeneration == Regeneration.Full)
        {
            NumberOfProbes = 1;
        }

        if (Regeneration == Regeneration.None || Regeneration == Regeneration.Full)
        {
            ExtractionTable = GetExtrationValues(NumberOfProbes);
        }
        if (Regeneration == Regeneration.Half)
        {
            ExtractionTable = GetExtrationValues_Regeneration(NumberOfProbes);
        }

        //Get Weights

        decimal LowerWeight = 1 - ((decimal)Conductivity - Math.Floor((decimal)Conductivity));
        decimal UpperWeight = (decimal)Conductivity - Math.Floor((decimal)Conductivity);

        //Value Selection

        int LowerValue = (int)Math.Floor((decimal)Conductivity);
        int UpperValue = (int)Math.Ceiling((decimal)Conductivity);

        var extractionMap = new Dictionary<int, double?>
        {
            { 1, ExtractionTable.WM1 },
            { 2, ExtractionTable.WM2 },
            { 3, ExtractionTable.WM3 },
            { 4, ExtractionTable.WM4 }
        };

        double? SelectLowerValue = extractionMap.GetValueOrDefault(LowerValue);
        double? SelectUpperValue = extractionMap.GetValueOrDefault(UpperValue);

        var WeightedResult = SelectLowerValue * (double)LowerWeight + SelectUpperValue * (double)UpperWeight;


        double? AdaptionValues = 1;

        if (FlowIsTurbolent == false)
        {
            AdaptionValues *= Flowrate(Conductivity);
        }

        //ThermoGrout Adeption

        var ApdaptedResult = WeightedResult * AdaptionValues;

        //Thermo Grout Adjsutment

        var PercentageIncrease = (0.5155 * ApdaptedResult - 0.6591) / 100 + 1;

        ApdaptedResult = ApdaptedResult * PercentageIncrease;

        return ApdaptedResult;

    }

    private (double? WM1, double? WM2, double? WM3, double? WM4) GetExtrationValues(int? n)
    {

        if (n == null) { throw new Exception("CE: Number of boreholes for the extraction calculation not given!"); }

        var WM1 = 21.9225 / Math.Pow((double)(n + 1.0589), 0.3706) + 4.1915;

        var WM2 = 40.5743 / Math.Pow((double)(n + 1.6282), 0.3723) + 4.3738;

        var WM3 = 64.4596 / Math.Pow((double)(n + 3.0587), 0.4543) + 6.9836;

        var WM4 = 80.4383 / Math.Pow((double)(n + 3.7651), 0.4573) + 8.2964;

        return (WM1, WM2, WM3, WM4);
    }

    private (double? WM1_reg, double? WM2_reg, double? WM3_reg, double? WM4_reg) GetExtrationValues_Regeneration(int? n)
    {
        if (n == null) { throw new Exception("CE: Number of boreholes for the extraction calculation not given!"); }

        var WM1_reg = 32.4208 / Math.Pow((double)(n + 4.2756), 0.4558) + 8.3406;

        var WM2_reg = 46.0563 / Math.Pow((double)(n + 3.8569), 0.3746) + 10.1330;

        var WM3_reg = 58.0435 / Math.Pow((double)(n + 4.4775), 0.3663) + 12.3952;

        var WM4_reg = 71.2317 / Math.Pow((double)(n + 5.4259), 0.3854) + 14.4587;

        return (WM1_reg, WM2_reg, WM3_reg, WM4_reg);
    }

    private double? Flowrate(double? con)
    {
        if (con == null) { return null; }
        return 0.005 * Math.Pow((double)con, 2) - 0.045 * con + 0.89;
    }



}