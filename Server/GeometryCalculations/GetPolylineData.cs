using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NetTopologySuite.Operation.Distance;
using Microsoft.EntityFrameworkCore;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetPolylineData
{
    Task<double?> GetNearestPolylineData(LandParcel landParcelElement);
}

public class GetPolylineData(DataContext context, IParameterDeserialator parameterDeserialator) : IGetPolylineData
{
    public async Task<double?> GetNearestPolylineData(LandParcel landParcelElement)
    {
        var polyLineId = await context.GeothermalParameter.Where(gp => gp.Geometry_Type == Geometry_Type.polyline)
            .Select(gp => new
            {
                gp.Type,
                gp.Id

            }).ToListAsync();

        //ExpectedGroundWaterHight Case

        var ExpectedGroundWaterHight_info = polyLineId.FirstOrDefault(pl => pl.Type == TypeOfData.expe_max_groundwater_hight);

        if(ExpectedGroundWaterHight_info?.Id == null || landParcelElement.Geometry == null) { throw new Exception("Geometry for Line data Not Found"); }

        var ExpectedGroundWaterHight_data = await context.GeoData
            .Where(gd => gd.ParameterKey == ExpectedGroundWaterHight_info.Id && gd.Parameter != null)
            .Select(gd => new
            {
                gd.Parameter,
                Distance = gd.Geom!.Distance(landParcelElement.Geometry.Centroid)
            })
            .OrderBy(gd => gd.Distance).Take(100)
            .ToListAsync();

        var Zehgw_m_tx_Element = ExpectedGroundWaterHight_data.Select(expec => new
        {
            parameter = parameterDeserialator.DeserializeParameters(expec.Parameter!).Zehgw_m_tx,
            Distance = expec.Distance
        }).Where(x => x.parameter != "").FirstOrDefault();


        if(Zehgw_m_tx_Element  == null || Zehgw_m_tx_Element.parameter == "" || Zehgw_m_tx_Element.parameter == null) { return null; }

        var zehgw = Convert.ToDouble(Regex.Replace(Zehgw_m_tx_Element.parameter, ",", "."), CultureInfo.InvariantCulture);

        return zehgw;
    }
}
//await Task.Run(() => parameterDeserialator.DeserializeParameters(UnserilizedDepthRestrictions?.Parameter ?? ""));
