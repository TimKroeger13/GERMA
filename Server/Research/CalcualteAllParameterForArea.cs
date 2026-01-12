using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using System.Linq;
using NetTopologySuite.IO;
using Microsoft.EntityFrameworkCore;
using GERMAG.Shared;
using GERMAG.Server.GeometryCalculations;
using System.Net;
using System.Text.RegularExpressions;
using GERMAG.Server.DataPulling;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Microsoft.AspNetCore.Http.Features;
using NetTopologySuite.Features;
using System.Text.Json;
using GERMAG.Shared.PointProperties;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GERMAG.Server.Research;

public interface ICalcualteAllParameterForArea
{
    Task<string> CalucalteAllParameters();
}

public class CalcualteAllParameterForArea() : ICalcualteAllParameterForArea //DataContext context, IFindLocalDirectoryPath findLocalDirectoryPath, IGeoThermalProbesCalcualtion geoThermalProbesCalcualtion, IGetProbeSpecificData getProbeSpecificData
{
    public async Task<String> CalucalteAllParameters()
    {
        await Task.CompletedTask;
        /*
        //Calcualte RestrictionArea

        Console.WriteLine("Starting scientific calcualtion");

        var inputPath = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinRestrictionFlaechen.geojson");

        NetTopologySuite.Features.FeatureCollection featureCollection;

        if (File.Exists(inputPath))
        {
            string jsonString = File.ReadAllText(inputPath);

            var reader = new GeoJsonReader();
            featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(jsonString);

            foreach (var elementGeometry in featureCollection)
            {
                elementGeometry.Geometry.SRID = 25833;
            }
        }
        else
        {
            //get geodata
            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(60));

            Console.WriteLine("Loading Select");
            var selectedData = context.AxSelecteds.ToList();
            Console.WriteLine("Loading Trees");
            var ax_tree = context.AxTrees.ToList();
            Console.WriteLine("Loading Buildings");
            var ax_buildings = context.AxBuildings.ToList();

            GeometryFactory geometryFactory = new();

            List<NetTopologySuite.Geometries.Geometry?> bufferedTrees = ax_tree.ConvertAll(tree => tree.Geom?.Buffer(OfficalParameters.TreeDistance));

            List<Polygon?> landParcelPolygon = selectedData.ConvertAll(selected => (Polygon?)selected.Geom);

            List<NetTopologySuite.Geometries.Geometry?> landParcelPolygonGeometry = selectedData.ConvertAll(selected => selected.Geom);

            List<LineString?> landParcelLineString = landParcelPolygon.ConvertAll(land => land?.ExteriorRing);

            List<NetTopologySuite.Geometries.Geometry?> bufferedLandParcel = landParcelLineString.ConvertAll(landlinestring => landlinestring?.Buffer(OfficalParameters.LandParcelDistance));

            List<NetTopologySuite.Geometries.Geometry?> bufferedBuldings = ax_buildings.ConvertAll(build => build.Geom?.Buffer(OfficalParameters.BuildingDistance));

            List<NetTopologySuite.Geometries.Geometry?> diffTree = CalcualteDifference(landParcelPolygonGeometry, bufferedTrees);

            List<NetTopologySuite.Geometries.Geometry?> diffTreeParcel = CalcualteDifference(diffTree, bufferedLandParcel);

            List<NetTopologySuite.Geometries.Geometry?> diffTreeParcelBuilding = CalcualteDifference(diffTreeParcel, bufferedBuldings);

            featureCollection = new NetTopologySuite.Features.FeatureCollection();

            var k = 0;
            foreach (var geometry in diffTreeParcelBuilding)
            {
                k++;
                Console.WriteLine(k + " / " + (diffTreeParcelBuilding.Count - 1));
                if (geometry != null && (geometry is Polygon || geometry is MultiPolygon))
                {
                    var properties = new AttributesTable();
                    properties.Add("SRID", 28533);

                    var feature = new NetTopologySuite.Features.Feature(geometry, properties);
                    featureCollection.Add(feature);
                }
            }

            var path = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinRestrictionFlaechen.geojson");

            var serializer = GeoJsonSerializer.Create();
            await using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, featureCollection);
                var geoJsonString = stringWriter.ToString();

                File.WriteAllText(path, geoJsonString);
            }
            context.Database.SetCommandTimeout(null);
        }

        // Quick restricted Area calculation

        double areaSum = 0;

        foreach (var element in featureCollection)
        {
            areaSum = areaSum + element.Geometry.Area;

        }

        var inputPathPoint = findLocalDirectoryPath.getLocalPath("CalculationResults", "berlinPointCalculation.geojson");

        NetTopologySuite.Features.FeatureCollection FeatureCollectionOfProbeGeometricPosistions;
        List<NetTopologySuite.Geometries.Geometry?> ProbeGeometricPosistions;


        if (File.Exists(inputPathPoint))
        {
            string jsonString = File.ReadAllText(inputPathPoint);

            var reader = new GeoJsonReader();
            FeatureCollectionOfProbeGeometricPosistions = reader.Read<NetTopologySuite.Features.FeatureCollection>(jsonString);

            foreach (var elementGeometry in featureCollection)
            {
                elementGeometry.Geometry.SRID = 25833;
            }

            ProbeGeometricPosistions = new List<NetTopologySuite.Geometries.Geometry?>();

            foreach (var feature in FeatureCollectionOfProbeGeometricPosistions)
            {
                var geometry = feature.Geometry;
                ProbeGeometricPosistions.Add(geometry);
            }

        }
        else
        {
            ProbeGeometricPosistions = await CalcualteProbePositionAsync(featureCollection);

            var featureCollectionPoints = new NetTopologySuite.Features.FeatureCollection();

            foreach (var geometry in ProbeGeometricPosistions)
            {
                if (geometry != null)
                {
                    var attributes = new AttributesTable();
                    var feature = new NetTopologySuite.Features.Feature(geometry, attributes);
                    featureCollectionPoints.Add(feature);
                }
            }

            var serializer = GeoJsonSerializer.Create();
            await using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, featureCollectionPoints);
                var geoJsonString = stringWriter.ToString();

                File.WriteAllText(inputPathPoint, geoJsonString);
            }

        }

         //Calcualte points in featureCollection

        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var landParcelElement = new LandParcel
        {
            ParameterKey = landParcelID,
        };

        List<ProbePoint?> probePointsList = ProbeGeometricPosistions.ConvertAll(geometry => new ProbePoint
        {
            Geometry = geometry,
        })!;

        List<ProbePoint?> FullProbePointInformation = await getProbeSpecificData.GetPointProbeData(landParcelElement, probePointsList);

        var MaxDepthList = FullProbePointInformation.ConvertAll(fppi => fppi?.Properties!.MaxDepth);

        var GeoPotenDepthList = FullProbePointInformation.ConvertAll(fppi => fppi?.Properties!.GeoPotenDepth);

        var GeoPotenList = FullProbePointInformation.ConvertAll(fppi => fppi?.Properties!.GeoPoten);

        var savePath = findLocalDirectoryPath.getLocalPath("CalculationResults", "MaxDepth.geojson");
        File.WriteAllText(savePath, JsonConvert.SerializeObject(MaxDepthList));

        savePath = findLocalDirectoryPath.getLocalPath("CalculationResults", "GeoPotenDepth.geojson");
        File.WriteAllText(savePath, JsonConvert.SerializeObject(GeoPotenDepthList));

        savePath = findLocalDirectoryPath.getLocalPath("CalculationResults", "GeoPoten.geojson");
        File.WriteAllText(savePath, JsonConvert.SerializeObject(GeoPotenList));
        */
        Console.WriteLine("Calculations are current out commented!");

        Console.WriteLine("All restricions for Berlin calculated!");

        return "Test from Server";
    }

    private async Task<List<NetTopologySuite.Geometries.Geometry?>> CalcualteProbePositionAsync(NetTopologySuite.Features.FeatureCollection featureCollection)
    {
        List<Task<List<ProbePoint?>>> tasks = new();

        var u = 0;

        foreach (var FeatureElement in featureCollection)
        {
            u++;
            Console.WriteLine(u);
/*            if (u >= 2)
            {
                break;
            }*/

            var featureGeometry = (NetTopologySuite.Geometries.Geometry?)FeatureElement.Geometry;

            var featureResrictionArea = new Restricion
            {
                Geometry_Usable = featureGeometry
            };

            //tasks.Add(geoThermalProbesCalcualtion.CalculateGeoThermalProbes(featureResrictionArea));
        }

        List<ProbePoint?>[] results = await Task.WhenAll(tasks);

        List<NetTopologySuite.Geometries.Geometry?> outputList = new();

        foreach (var result in results)
        {
            outputList.AddRange(result.Select(r => r?.Geometry));
        }

        return outputList;
    }
    private List<NetTopologySuite.Geometries.Geometry?> CalcualteDifference(List<NetTopologySuite.Geometries.Geometry?> sourcePolygon, List<NetTopologySuite.Geometries.Geometry?> differenceList)
    {
        GeometryFactory geometryFactory = new();

        for (var i = 0; i < sourcePolygon.Count; i++)
        {
            Console.WriteLine(i + " / " + (sourcePolygon.Count - 1));

            var single = sourcePolygon[i];

            if (single == null) {
                Console.WriteLine("Empty Element");
                continue; }

            List<NetTopologySuite.Geometries.Geometry?> intersectingGeometry = differenceList.Where(e => e!.Intersects(single)).ToList();

            if (intersectingGeometry.Count == 0) { continue; }

            NetTopologySuite.Geometries.Geometry intersectingGeometryUnion = geometryFactory.BuildGeometry(intersectingGeometry).Union();

            sourcePolygon[i] = single.Difference(intersectingGeometryUnion);
        }

        return sourcePolygon;
    }
}