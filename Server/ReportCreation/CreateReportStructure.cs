using GERMAG.Server.ExtensionMethods;
using GERMAG.Shared;
using Microsoft.Extensions.ObjectPool;
using NetTopologySuite.Geometries;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace GERMAG.Server.ReportCreation;

public interface ICreateReportStructure
{
    Report[] CreateReport(List<GeometryElementParameter> CoordinateParameters);
}

public class CreateReportStructure : ICreateReportStructure
{
    private readonly List<String>? _geo_poten_100m_with_2400ha = [];
    private readonly List<String>? _geo_poten_80m_with_2400ha = [];
    private readonly List<String>? _geo_poten_60m_with_2400ha = [];
    private readonly List<String>? _geo_poten_40m_with_2400ha = [];
    private readonly List<String>? _geo_poten_100m_with_1800ha = [];
    private readonly List<String>? _geo_poten_80m_with_1800ha = [];
    private readonly List<String>? _geo_poten_60m_with_1800ha = [];
    private readonly List<String>? _geo_poten_40m_with_1800ha = [];
    private readonly List<String>? _thermal_con_100 = [];
    private readonly List<String>? _thermal_con_80 = [];
    private readonly List<String>? _thermal_con_60 = [];
    private readonly List<String>? _thermal_con_40 = [];
    private readonly List<String>? _mean_water_temp_20to100 = [];
    private readonly List<String>? _mean_water_temp_80 = [];
    private readonly List<String>? _mean_water_temp_60 = [];
    private readonly List<String>? _mean_water_temp_40 = [];
    private readonly List<String>? _mean_water_temp_20 = [];
    private readonly List<String>? _geo_poten_restrict = [];
    private readonly List<String>? _holsteinRestrictionZone = [];
    private readonly List<String>? _protectionAreas = [];
    private string? _land_parcel_number = "";
    private string? _land_parcels_gemeinde = "";
    //private String? _geometry = null;
    private readonly String? _building_begzgkt = "";
    private string? _zeHGW = "";
    private readonly List<String>? _verordnung = [];
    private readonly List<String>? _veror_link = [];
    private readonly List<String>? _holstein_depth = [];
    private readonly List<String>? _rup_depth = [];
    private bool? _activeRestriction = false;
    private Regex protectionRex = new Regex("schutz", RegexOptions.IgnoreCase);
    public Report[] CreateReport(List<GeometryElementParameter> CoordinateParameters)
    {
        foreach (var CoordinateParameter in CoordinateParameters)
        {
            switch (CoordinateParameter.Type)
            {
                case DataModel.Database.TypeOfData.land_parcels:
                    _land_parcel_number = AppendString(_land_parcel_number ?? "", CompbineLandParcelNumbers(CoordinateParameter.JsonDataParameter?.Zae?.ToString() ?? "", CoordinateParameter.JsonDataParameter?.Nen?.ToString() ?? "") ?? "");
                    _land_parcels_gemeinde = AppendString(_land_parcels_gemeinde ?? "", CoordinateParameter.JsonDataParameter?.Namgem?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_restrict:
                    _geo_poten_restrict?.Add(CoordinateParameter.JsonDataParameter?.Text?.ToString() ?? "");
                    _activeRestriction = protectionRex.IsMatch(CoordinateParameter!.JsonDataParameter?.Text?.ToString() ?? string.Empty);
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_20to100:
                    _mean_water_temp_20to100?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_80:
                    _mean_water_temp_80?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_60:
                    _mean_water_temp_60?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_40:
                    _mean_water_temp_40?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_20:
                    _mean_water_temp_20?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_100m_with_2400ha:
                    _geo_poten_100m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_100txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_80m_with_2400ha:
                    _geo_poten_80m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_60m_with_2400ha:
                    _geo_poten_60m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_40m_with_2400ha:
                    _geo_poten_40m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_100m_with_1800ha:
                    _geo_poten_100m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_100xt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_80m_with_1800ha:
                    _geo_poten_80m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_60m_with_1800ha:
                    _geo_poten_60m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_40m_with_1800ha:
                    _geo_poten_40m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_40:
                    _thermal_con_40?.Add(CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_60:
                    _thermal_con_60?.Add(CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_80:
                    _thermal_con_80?.Add(CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_100:
                    _thermal_con_100?.Add(CoordinateParameter.JsonDataParameter?.La_100txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.water_protec_areas:
                    _verordnung?.Add(CoordinateParameter.JsonDataParameter?.Verordnung?.ToString() ?? "");
                    _veror_link?.Add(CoordinateParameter.JsonDataParameter?.Veror_link?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.building_surfaces:
                    //_building_begzgkt = AppendString(_building_begzgkt ?? "", CoordinateParameter.JsonDataParameter?.Bezgfk?.ToString() ?? "");
                    break;
                /*case DataModel.Database.TypeOfData.depth_restrictions:
                    _holstein_depth?.Add(CoordinateParameter.JsonDataParameter?.VALUE?.ToString() ?? "");
                    break;*/
                case DataModel.Database.TypeOfData.geologic_sections_berlin:
                    _holstein_depth?.Add(Math.Abs((decimal)(CoordinateParameter.JsonDataParameter?.Holstein_gok ?? 0)).ToString() ?? "");
                    _rup_depth?.Add(Math.Abs((decimal)(CoordinateParameter.JsonDataParameter?.Rupel_gok ?? 0)).ToString() ?? "");
                    break;
                /*case DataModel.Database.TypeOfData.depth_restrictions_rup:
                    _rup_depth?.Add(Math.Abs((decimal)(CoordinateParameter.JsonDataParameter?.Rupel_gok ?? 0)).ToString() ?? ""); 
                    break;*/
                //Cases Not relevant for the Report or right now not implemented
                case DataModel.Database.TypeOfData.groundwater_measuring_points:
                    break;
                case DataModel.Database.TypeOfData.dgm:
                    break;
                case DataModel.Database.TypeOfData.veg_height:
                    break;
                case DataModel.Database.TypeOfData.main_water_lines:
                    break;
                case DataModel.Database.TypeOfData.groundwater_surface_distance:
                    break;
                case DataModel.Database.TypeOfData.ground_water_height_main:
                    break;
                case DataModel.Database.TypeOfData.ground_water_height_tension:
                    break;
                case DataModel.Database.TypeOfData.water_ammonium:
                    break;
                case DataModel.Database.TypeOfData.water_bor:
                    break;
                case DataModel.Database.TypeOfData.water_chlor:
                    break;
                case DataModel.Database.TypeOfData.water_kalium:
                    break;
                case DataModel.Database.TypeOfData.water_sulfat:
                    break;
                case DataModel.Database.TypeOfData.water_ortho_phosphat:
                    break;
                case DataModel.Database.TypeOfData.electrical_con:
                    break;
                case DataModel.Database.TypeOfData.geodrilling_data:
                    break;
                case DataModel.Database.TypeOfData.geological_sections:
                    break;
                case DataModel.Database.TypeOfData.geo_drawing:
                    break;
                case DataModel.Database.TypeOfData.expe_max_groundwater_hight:
                    _zeHGW = AppendString(_zeHGW ?? "", CoordinateParameter.JsonDataParameter?.Zehgw_m_tx?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.holstein_restrictions:
                    _holsteinRestrictionZone?.Add(CoordinateParameter.JsonDataParameter?.Text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.protection_area:
                    _protectionAreas?.Add(CoordinateParameter.JsonDataParameter?.Layer?.ToString() ?? "");
                    break;
                case null:
                    break;
            }
        }

        var report = new[] { new Report
        {
            Holstein = CheckHolsteinRestrictions(_holstein_depth?.ConvertDocumentationString(0) ?? "",_holsteinRestrictionZone ?? []),
            Rupelton = _rup_depth?.ConvertDocumentationString(1),
            TotalMaxDepth = CalculateMaxDepth(_holstein_depth ?? [], _rup_depth ?? [], _holsteinRestrictionZone ?? []),
            Geo_poten_100m_with_2400ha = _geo_poten_100m_with_2400ha?.ConvertDocumentationString(2),
            Geo_poten_80m_with_2400ha = _geo_poten_80m_with_2400ha?.ConvertDocumentationString(2),
            Geo_poten_60m_with_2400ha = _geo_poten_60m_with_2400ha?.ConvertDocumentationString(2),
            Geo_poten_40m_with_2400ha = _geo_poten_40m_with_2400ha?.ConvertDocumentationString(2),
            Geo_poten_100m_with_1800ha = _geo_poten_100m_with_1800ha?.ConvertDocumentationString(2),
            Geo_poten_80m_with_1800ha = _geo_poten_80m_with_1800ha?.ConvertDocumentationString(2),
            Geo_poten_60m_with_1800ha = _geo_poten_60m_with_1800ha?.ConvertDocumentationString(2),
            Geo_poten_40m_with_1800ha = _geo_poten_40m_with_1800ha?.ConvertDocumentationString(2),
            Thermal_con_100 = _thermal_con_100?.ConvertDocumentationString(2),
            Thermal_con_80 = _thermal_con_80?.ConvertDocumentationString(2),
            Thermal_con_60 = _thermal_con_60?.ConvertDocumentationString(2),
            Thermal_con_40 = _thermal_con_40?.ConvertDocumentationString(2),
            Mean_water_temp_20to100 = _mean_water_temp_20to100?.ConvertDocumentationString(2),
            Mean_water_temp_80 = _mean_water_temp_80?.ConvertDocumentationString(2),
            Mean_water_temp_60 = _mean_water_temp_60?.ConvertDocumentationString(2),
            Mean_water_temp_40 = _mean_water_temp_40?.ConvertDocumentationString(2),
            Mean_water_temp_20 = _mean_water_temp_20?.ConvertDocumentationString(2),
            Geo_poten_restrict = _geo_poten_restrict?.ToUniqueStringList(),
            Verordnung = _verordnung?.ToUniqueStringList(),
            Veror_link = _veror_link?.ToUniqueStringList(),
            Land_parcel_number = _land_parcel_number,
            Land_parcels_gemeinde = _land_parcels_gemeinde,
            Building_begzgkt = _building_begzgkt,
            ActiveRestriction = _activeRestriction,
            ProtecitonList = CleanProtectionAreas(_protectionAreas)
            //ZeHGW = _zeHGW <- polylines
        } };

        return report;
    }

    private static String CalculateMaxDepth(List<String> _holstein_depth, List<String> _rup_depth, List<String> _holsteinRestrictionZone)
    {
        if (_holstein_depth.Count() == 0 && _rup_depth.Count() == 0) { return OfficalParameters.DepthFactorMax.ToString(); }

        List<double> Rup_list = [];
        List<double> hol_list = [];

        if (_holstein_depth.Count() == 0) { _holstein_depth = new List<string> { OfficalParameters.DepthFactorMax.ToString() }; }
        if (_rup_depth.Count() == 0) { _rup_depth = new List<string> { OfficalParameters.DepthFactorMax.ToString() }; }

        foreach (var rupElement in _rup_depth)
        {
            Rup_list.Add(double.Parse(rupElement));
        }

        var Rup_list_min = Rup_list.Min();

        foreach (var holElement in _holstein_depth)
        {
            hol_list.Add(double.Parse(holElement));
        }

        var Hol_list_min = hol_list.Min();

        if (_holsteinRestrictionZone.Count == 0)
        {
            Hol_list_min = OfficalParameters.DepthFactorMax;
        }

        var MinimalMaxDepth = Math.Min(Math.Min(Rup_list_min, Hol_list_min), OfficalParameters.DepthFactorMax);

        if (MinimalMaxDepth > 100)
        {
            MinimalMaxDepth -= MinimalMaxDepth % OfficalParameters.DepthChunkValue;
        }
        else
        {
            MinimalMaxDepth -= MinimalMaxDepth % OfficalParameters.ShallowChunkValue;
        }

        return MinimalMaxDepth.ToString();

    }

    private static String CheckHolsteinRestrictions(String _holstein_depth, List<String> _holsteinRestrictionZone)
    {

        if (_holsteinRestrictionZone.Count == 0)
        {
            return "";
        }
        return _holstein_depth;

    }

    private static String AppendString(String OriginString, String NewString)
    {
        if (string.IsNullOrEmpty(OriginString))
        {
            return NewString;
        }
        else
        {
            return $"{OriginString}, {NewString}";
        }
    }

    private static List<String>? CleanProtectionAreas(List<String>? protectionList)
    {
        if (protectionList == null) { return null; }

        var distinctProtectionList = protectionList.Distinct().ToList();

        var UppercaseDistinctProtectionList = distinctProtectionList.Select(entry => char.ToUpper(entry[0]) + entry.Substring(1)).ToList();

        return UppercaseDistinctProtectionList;
    }

    private static string? CompbineLandParcelNumbers(string? zeh, string? nen)
    {
        if(nen != null)
        {
            return(zeh + "/" + nen);
        }
        else
        {
            return (zeh);
        }
    }
}
