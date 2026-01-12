using GERMAG.DataModel.Database;
using GERMAG.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using Npgsql;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Geometries.Prepared;
using GERMAG.Server.ReportCreation;

namespace GERMAG.Server.GeometryCalculations;

public interface IReceiveLandParcel
{
    Task<LandParcel> GetLandParcel(List<double> Xcor, List<double> Ycor, int Srid);
}

public class ReceiveLandParcel(DataContext context, IParameterDeserialator parameterDeserialator) : IReceiveLandParcel
{
    public async Task<LandParcel> GetLandParcel(List<double> Xcor, List<double> Ycor, int Srid)
    {
        return await Task.Run(() =>
        {
            var geoJsonWriter = new GeoJsonWriter();

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);
            var geometryFactoryTarget = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 25833);

            List<NetTopologySuite.Geometries.Geometry> GeometryList = new List<NetTopologySuite.Geometries.Geometry>();

            for (var i = 0; i < Xcor.Count; i++)
            {
                var convertedPoint = geometryFactory.CreatePoint(new Coordinate(Xcor[i], Ycor[i]));

                var transformedPoint = context.GeoData  //Database
                .FromSql($"SELECT ST_Transform(ST_SetSRID(ST_MakePoint({convertedPoint.X}, {convertedPoint.Y}), {Srid}), 25833) AS geom")
                .Select(gd => gd.Geom)
                .FirstOrDefault();

                if (transformedPoint != null)
                {
                    GeometryList.Add(transformedPoint);
                }
            }
            var pointsArray = GeometryList.OfType<Point>().ToArray();

            // Create a MultiPoint from the array of Points.
            MultiPoint multiPointList = geometryFactoryTarget.CreateMultiPoint(pointsArray);

            NetTopologySuite.Geometries.Prepared.IPreparedGeometry multiPointListPrepared = PreparedGeometryFactory.Prepare(multiPointList);

            //var originalPoint = geometryFactory.CreatePoint(new Coordinate(Xcor[0], Ycor[0]));

            /*var transformedPoint = context.GeoData  //Database
                .FromSql($"SELECT ST_Transform(ST_SetSRID(ST_MakePoint({originalPoint.X}, {originalPoint.Y}), {Srid}), 25833) AS geom")
                .Select(gd => gd.Geom)
                .FirstOrDefault();*/

            var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

            var landparcelIntersection = context.GeoData.Where(gd => gd.ParameterKey == landParcelID && gd.Geom!.Intersects(multiPointListPrepared.Geometry)).Select(gd => new { gd.Geom, gd.Id, gd.ParameterKey, gd.Parameter }).ToList();

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


            NetTopologySuite.Geometries.Geometry? unionGeometry = null;

            foreach (var intersection in landparcelIntersection)
            {
                if (intersection.Geom == null)
                {
                    continue;
                }

                unionGeometry = unionGeometry == null ? intersection.Geom : unionGeometry.Union(intersection.Geom);
            }


            if (landparcelIntersection.Count == 0)
            {
                return new LandParcel
                {
                    Error = true,
                };
            }

            var returnValue = new LandParcel
            {
                GeoDataID = landparcelIntersection[0].Id,
                ParameterKey = landParcelID,
                Parameter = landparcelIntersection[0].Parameter,
                Geometry = unionGeometry,
                GeometryJson = geoJsonWriter.Write(unionGeometry),
                LandParcels = JoinedListOfLandsParcels
            };

            return returnValue;
        });
    }
}