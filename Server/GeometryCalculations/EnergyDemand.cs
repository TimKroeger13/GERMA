using GERMAG.Shared;
using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json;

namespace GERMAG.Server.GeometryCalculations;

public interface IEnergyDemand
{
    Task<EnergyMapDemandData> GetEneryDemand(LandParcel landParcelElement);
}

public class EnergyDemand(IGeometryTransformation geometryTransformation) : IEnergyDemand
{
    public async Task<EnergyMapDemandData> GetEneryDemand(LandParcel landParcelElement)
    {
        //Extend Buffer to Find all Buildings. Building are only found when they are covered by the Polygon

        var BufferedLandParcelElement = landParcelElement.Geometry?.Buffer(5);
        
        //Transform it back into 

        var TransformedLandParcelElement = await geometryTransformation.TransformGeometryBack(BufferedLandParcelElement ?? throw new Exception ("CE: Geometry for Backtransformation is not given") ,25833);

        var GeometryToSendGeoJson = TransformedLandParcelElement.Geometry;

        //Create Getrequest String

        var baseUrl = $"https://energymap-berlin.de/map/query?mode=polygon&linestring=";

        var coordinateList = new List<string>();

        foreach (var coordinate in GeometryToSendGeoJson!.Coordinates)
        {
            coordinateList.Add($"{coordinate.X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{coordinate.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        var linestringParam = string.Join(",", coordinateList);

        var getRequestUrl = baseUrl + linestringParam;

        //Send Getrequest

        string? responseBody = null;

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(getRequestUrl);

                response.EnsureSuccessStatusCode(); // Throw an exception if not successful
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                //Console.WriteLine($"Request error: {e.Message}");
            }
        }

        //Request to long or error

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            //throw new Exception("CE: Response body from EnergyMap is empty!");

            return new EnergyMapDemandData
            {
                ActInsolation = 0,
                BetterInsolation = 0,
                OptimalInsolation = 0,
            };
        }

        var EnergyMapObjectList = JsonSerializer.Deserialize<List<EnergyMapProperties>>(responseBody ?? "", new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new Exception("No EnergyMap API found");

        int TotalEnergyDemand_ActInsolation = 0;
        int TotalEnergyDemand_BetterInsolation = 0;
        int TotalEnergyDemand_OptimalInsolation = 0;

        foreach (var bulding in EnergyMapObjectList)
        {
            TotalEnergyDemand_ActInsolation += (int)Math.Round(Convert.ToDouble(bulding.Cons_c1r1,  System.Globalization.CultureInfo.InvariantCulture), 0);
            TotalEnergyDemand_BetterInsolation += (int)Math.Round(Convert.ToDouble(bulding.Cons_c1r2,  System.Globalization.CultureInfo.InvariantCulture), 0);
            TotalEnergyDemand_OptimalInsolation += (int)Math.Round(Convert.ToDouble(bulding.Cons_c1r3,  System.Globalization.CultureInfo.InvariantCulture), 0);
        }

        var returnValue = new EnergyMapDemandData
        {
            ActInsolation = TotalEnergyDemand_ActInsolation,
            BetterInsolation = TotalEnergyDemand_BetterInsolation,
            OptimalInsolation = TotalEnergyDemand_OptimalInsolation,
        };

        return returnValue;

    }


}