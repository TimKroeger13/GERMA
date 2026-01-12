using NetTopologySuite.Geometries;
using System;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NetTopologySuite.CoordinateSystems.Transformations;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GeoAPI.CoordinateSystems;
using GERMAG.DataModel.Database;
using GERMAG.Shared;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeometryTransformation
{
    Task<LandParcel> TransformGeometry(NetTopologySuite.Geometries.Geometry geometry, int srid);
    Task<(List<double> Xs, List<double> Ys)> DirectTransformation(List<double> longitudes, List<double> latitudes, int sourceSrid, int targetSrid);
    Task<LandParcel> TransformGeometryBack(NetTopologySuite.Geometries.Geometry geometry, int sourceSrid);
    
}

public class GeometryTransformation(DataContext context) : IGeometryTransformation
{
    public async Task<LandParcel> TransformGeometry(NetTopologySuite.Geometries.Geometry geometry, int SourceSrid)
    {
        var TagedSrid = 25833;
        var geoJsonWriter = new GeoJsonWriter();

        if (geometry is NetTopologySuite.Geometries.Polygon polygon)
        {
            // Transform the shell (outer ring)
            var transformedShell = await TransformLinearRing(polygon.Shell, SourceSrid, TagedSrid);

            // Transform each hole (inner rings)
            var transformedHoles = new List<LinearRing>();
            foreach (var hole in polygon.Holes)
            {
                var transformedHole = await TransformLinearRing(hole, SourceSrid, TagedSrid);
                transformedHoles.Add(transformedHole);
            }

            // Build the transformed polygon
            geometry = new Polygon(transformedShell, transformedHoles.ToArray())
            {
                SRID = TagedSrid
            };

            geometry.SRID = TagedSrid;
        }
        else if (geometry is NetTopologySuite.Geometries.MultiPolygon multiPolygon)
        {
            var transformedPolygons = new List<Polygon>();
            
            foreach (Polygon poly in multiPolygon.Geometries)
            {
                // Transform the shell
                var transformedShell = await TransformLinearRing(poly.Shell, SourceSrid, TagedSrid);
                
                // Transform holes
                var transformedHoles = new List<LinearRing>();
                foreach (var hole in poly.Holes)
                {
                    var transformedHole = await TransformLinearRing(hole, SourceSrid, TagedSrid);
                    transformedHoles.Add(transformedHole);
                }
                
                transformedPolygons.Add(new Polygon(transformedShell, transformedHoles.ToArray()));
            }
            
            geometry = new MultiPolygon(transformedPolygons.ToArray())
            {
                SRID = TagedSrid
            };
        }

        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var returnResult = new LandParcel
        {
            GeoDataID = landParcelID,
            ParameterKey = landParcelID,
            Parameter = "",
            Geometry = geometry,
            GeometryJson = geoJsonWriter.Write(geometry),
        };

        return returnResult;
    }

    public async Task<LandParcel> TransformGeometryBack(NetTopologySuite.Geometries.Geometry geometry, int sourceSrid)
    {
        var targetSrid = 4326;
        var geoJsonWriter = new GeoJsonWriter();

        if (geometry is NetTopologySuite.Geometries.Polygon || 
            geometry is NetTopologySuite.Geometries.MultiPolygon)
        {
            for (int i = 0; i < geometry.Coordinates.Length; i++)
            {
                var rawCoordinate = geometry.Coordinates[i];
                var transformed = await transformCoordinates(rawCoordinate.X, rawCoordinate.Y, sourceSrid, targetSrid)
                                ?? throw new Exception("Coordinates could not be transformed back");

                geometry.Coordinates[i].X = transformed[0];
                geometry.Coordinates[i].Y = transformed[1];
            }
        }

        geometry.SRID = targetSrid;

        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        return new LandParcel
        {
            GeoDataID = landParcelID,
            ParameterKey = landParcelID,
            Parameter = "",
            Geometry = geometry,
            GeometryJson = geoJsonWriter.Write(geometry),
        };
    }

    public async Task<(List<double> Xs, List<double> Ys)> DirectTransformation(List<double> longitudes, List<double> latitudes, int sourceSrid, int targetSrid)
    {
        if (longitudes.Count != latitudes.Count)
            throw new ArgumentException("Longitude and latitude lists must have the same number of elements.");

        var transformedXs = new List<double>();
        var transformedYs = new List<double>();

        for (int i = 0; i < longitudes.Count; i++)
        {
            var lon = longitudes[i];
            var lat = latitudes[i];

            var result = await transformCoordinates(lon, lat, sourceSrid, targetSrid)
                         ?? throw new Exception($"Failed to transform coordinate at index {i}");

            transformedXs.Add(result[0]);
            transformedYs.Add(result[1]);
        }

        return (transformedXs, transformedYs);
    }



    private async Task<LinearRing> TransformLinearRing(LinearRing ring, int sourceSrid, int targetSrid)
    {
        var transformedCoordinates = new List<NetTopologySuite.Geometries.Coordinate>();

        foreach (var coord in ring.Coordinates)
        {
            var transformed = await transformCoordinates(coord.X, coord.Y, sourceSrid, targetSrid)
                            ?? throw new Exception("Coordinate transformation failed");

            transformedCoordinates.Add(new NetTopologySuite.Geometries.Coordinate(transformed[0], transformed[1]));
        }

        return new LinearRing(transformedCoordinates.ToArray());
    }


    async private Task<double[]?> transformCoordinates(double lon, double lat, int SourceSrid, int TagedSrid)
    {
        return await Task.Run(() =>
        {
            if (SourceSrid == 4326 && TagedSrid == 25833)
            {
                return CoordinateSystemWkt.Transformation_4326_to_25833.MathTransform.Transform(new double[] { lon, lat });
            }

            if (SourceSrid == 25833 && TagedSrid == 4326)
            {
                return CoordinateSystemWkt.Transformation_25833_to_4326.MathTransform.Transform(new double[] { lon, lat });
            }

            return null;
        });
    }

    private static class CoordinateSystemWkt
    {
        public static readonly CoordinateSystemFactory CsFactory;
        public static readonly CoordinateTransformationFactory CtFactory;
        public static readonly CoordinateSystem Wgs84;
        public static readonly CoordinateSystem Utm33n;
        public static readonly ICoordinateTransformation Transformation_4326_to_25833;
        public static readonly ICoordinateTransformation Transformation_25833_to_4326;


        public static string Wgs84String = "GEOGCS[\"WGS 84\", " +
            "DATUM[\"WGS_1984\", " +
            "SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]], " +
            "AUTHORITY[\"EPSG\",\"6326\"]], " +
            "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]], " +
            "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]], " +
            "AUTHORITY[\"EPSG\",\"4326\"]]";

        public static string Utm33nString = "PROJCS[\"ETRS89 / UTM zone 33N\", " +
            "GEOGCS[\"ETRS89\", " +
            "DATUM[\"European_Terrestrial_Reference_System_1989\", " +
            "SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]], " +
            "AUTHORITY[\"EPSG\",\"6258\"]], " +
            "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]], " +
            "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]], " +
            "AUTHORITY[\"EPSG\",\"4258\"]], " +
            "PROJECTION[\"Transverse_Mercator\"], " +
            "PARAMETER[\"latitude_of_origin\",0], " +
            "PARAMETER[\"central_meridian\",15], " +
            "PARAMETER[\"scale_factor\",0.9996], " +
            "PARAMETER[\"false_easting\",500000], " +
            "PARAMETER[\"false_northing\",0], " +
            "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]], " +
            "AUTHORITY[\"EPSG\",\"25833\"]]";

        static CoordinateSystemWkt()
        {
            CsFactory = new CoordinateSystemFactory();
            CtFactory = new CoordinateTransformationFactory();

            Wgs84 = (CoordinateSystem)CsFactory.CreateFromWkt(Wgs84String);
            Utm33n = (CoordinateSystem)CsFactory.CreateFromWkt(Utm33nString);

            Transformation_4326_to_25833 = CtFactory.CreateFromCoordinateSystems(Wgs84, Utm33n);
            Transformation_25833_to_4326 = CtFactory.CreateFromCoordinateSystems(Utm33n, Wgs84);
        }
    }
}
