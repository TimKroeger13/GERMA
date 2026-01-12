using AutoMapper.Features;
using GERMAG.DataModel.Database;
using GERMAG.Shared;
using System.Linq;
using System.Text.Json;

namespace GERMAG.Server.DataPulling.JsonDeserialize;

public interface IJsonDeserialize
{
    Root DeserializeJson(string SerializedInputJson, TypeOfData typeOfData, JsonFormat format);
}

public class JsonDeserialize() : IJsonDeserialize
{
    public Root DeserializeJson(string SerializedInputJson, TypeOfData typeOfData, JsonFormat format)
    {
        Root jsonData_Root = JsonSerializer.Deserialize<Root>(SerializedInputJson, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new Exception("No wfs found (root)");

        if (format == JsonFormat.long_coordiantes)
        {
            if (jsonData_Root.Features?.Count > 0)
            {
                foreach (var feature in jsonData_Root.Features)
                {
                    if (feature.Geometry != null)
                    {
                        if (feature.Geometry.Coordinates == null && feature.Geometry.CoordinatesLong != null)
                        {
                            feature.Geometry.Coordinates = CopyCoordinateLongsToCoordinates(feature.Geometry.CoordinatesLong);
                            feature.Geometry.CoordinatesLong = null;
                        }
                    }
                }
            }
        }
        if (format == JsonFormat.short_coordiantes)
        {
            if (jsonData_Root.Features?.Count > 0)
            {
                foreach (var feature in jsonData_Root.Features)
                {
                    if (feature.Geometry != null)
                    {
                        if (feature.Geometry.Coordinates == null && feature.Geometry.CoordinatesShort != null)
                        {
                            feature.Geometry.Coordinates = CopyCoordinateShortToCoordinates(feature.Geometry.CoordinatesShort);
                            feature.Geometry.CoordinatesShort = null;
                        }
                    }
                }
            }
        }
        if (format == JsonFormat.single_coordiantes)
        {
            if (jsonData_Root.Features?.Count > 0)
            {
                foreach (var feature in jsonData_Root.Features)
                {
                    if (feature.Geometry != null)
                    {
                        if (feature.Geometry.Coordinates == null && feature.Geometry.CoordinatesSingle != null)
                        {
                            var doubleList = feature.Geometry.CoordinatesSingle;
                            feature.Geometry.Coordinates = [[feature.Geometry.CoordinatesSingle!]];
                            feature.Geometry.CoordinatesSingle = null;
                        }
                    }
                }
            }
        }
        return jsonData_Root;
    }
    private static List<List<List<double>>>? CopyCoordinateLongsToCoordinates(List<List<List<List<double>>>>? coordinateLongs)
    {
        if (coordinateLongs == null || coordinateLongs.Count == 0)
            return null;
        return coordinateLongs[0];
    }
    private List<List<List<double>>>? CopyCoordinateShortToCoordinates(List<List<double>>? coordinateShort)
    {
        return [coordinateShort!];
    }
}
