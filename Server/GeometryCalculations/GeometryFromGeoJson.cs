using NetTopologySuite.IO;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using GERMAG.DataModel.Database;
using GERMAG.Shared;
using NetTopologySuite.Triangulate.QuadEdge;
using NetTopologySuite.Geometries.Prepared;
using GERMAG.Server.ReportCreation;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeometryFromGeoJson
{
    Task<LandParcel> GetGeometryFromgeoJson(string Geojson, int srid);
    Task<List<LandParcel>?> GetPolygonsFromGeoJson(string geoJson, int srid);
}

public class GeometryFromGeoJson(DataContext context, IGeometryTransformation geometryTransformation, IParameterDeserialator parameterDeserialator) : IGeometryFromGeoJson
{
    public async Task<LandParcel> GetGeometryFromgeoJson(string Geojson, int srid)
    {

        var serializer = GeoJsonSerializer.Create();

        // Deserialize the GeoJSON string to a Feature
        NetTopologySuite.Features.Feature? feature;

        try
        {
            using (var stringReader = new System.IO.StringReader(Geojson))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
            }
        }
        catch (InvalidCastException)
        {
            //Multipolygon that only contains one Element correction case correction
            string correctedGeoJson = Geojson.Replace("\"type\":\"MultiPolygon\"", "\"type\":\"Polygon\"");
            
            using (var stringReader = new System.IO.StringReader(correctedGeoJson))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
            }
        }

        if (feature == null || feature.Geometry == null)
        {
            throw new Exception("Geometry string could not be converted into geometry");
        }

        // Get the Geometry from the Feature
        NetTopologySuite.Geometries.Geometry? geometry = feature.Geometry;

        LandParcel landParcelElement = await geometryTransformation.TransformGeometry(geometry, srid);

        NetTopologySuite.Geometries.Prepared.IPreparedGeometry preparedBufferedGeometry = PreparedGeometryFactory.Prepare(landParcelElement.Geometry?.Buffer(-1));

        //Prepare new LandparcelData
        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == landParcelID && gd.Geom!.Intersects(preparedBufferedGeometry.Geometry)).Select(gd => new { gd.Geom, gd.Id, gd.ParameterKey, gd.Parameter }).ToList();

        List<string> listOfLandsParcels = [];

        foreach (var singleLandParcel in landparcelIntersection)
        {
            Shared.Properties DeserilizedLandParcelNumber = parameterDeserialator.DeserializeParameters(singleLandParcel?.Parameter ?? "");

            if(DeserilizedLandParcelNumber.Nen != "")
            {
                listOfLandsParcels.Add(DeserilizedLandParcelNumber.Zae + "/" + DeserilizedLandParcelNumber.Nen ?? "");
            }
            else
            {
                listOfLandsParcels.Add(DeserilizedLandParcelNumber.Zae ?? "");
            }

        }// Here Join Strings

        var JoinedListOfLandsParcels = listOfLandsParcels.Aggregate((current, next) => current + "," + next);

        //Intect new LandParcel Data
        landParcelElement.LandParcels = JoinedListOfLandsParcels;

        return landParcelElement;
    }

    public async Task<List<LandParcel>?> GetPolygonsFromGeoJson(string geoJson, int srid)
    {
        if (string.IsNullOrWhiteSpace(geoJson)) { return null; }

        var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create();

        NetTopologySuite.Features.FeatureCollection? featureCollection;

        using (var stringReader = new System.IO.StringReader(geoJson))
        using (var jsonReader = new Newtonsoft.Json.JsonTextReader(stringReader))
        {
            featureCollection = serializer.Deserialize<NetTopologySuite.Features.FeatureCollection>(jsonReader);
        }

        if (featureCollection == null || featureCollection.Count == 0)
        {
            return null;
        }

        // Extract geometries
        var geometries = featureCollection
            .Select(f => f.Geometry)
            .Where(g => g != null)
            .ToList();

        List<LandParcel> ListOfReprojectedNegativeAreas = [];

        foreach (var geom in geometries)
        {
            ListOfReprojectedNegativeAreas.Add(await geometryTransformation.TransformGeometry(geom, srid));
        }

        return ListOfReprojectedNegativeAreas;
    }


}

