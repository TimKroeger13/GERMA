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

public interface IReciveBoundingBoxInformation
{
    Task<BoundingBoxData?> GetBoundingBoxInformation((double, double) TL, (double, double) BR, int Srid);
}

public class ReciveBoundingBoxInformation(DataContext context, IGeometryTransformation geometryTransformation) : IReciveBoundingBoxInformation
{
    public async Task<BoundingBoxData?> GetBoundingBoxInformation((double, double) TL, (double, double) BR, int Srid)
    {

        var geoJsonWriter = new GeoJsonWriter();

        //var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);
        var geometryFactoryTarget = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 25833);

        List<NetTopologySuite.Geometries.Geometry> GeometryList = new List<NetTopologySuite.Geometries.Geometry>();

        //Get Bounding Box

        var x_list = new List<double> { TL.Item1, BR.Item1 };
        var y_list = new List<double> { TL.Item2, BR.Item2 };

        (List<double> Xs, List<double> Ys) TransformedCoordianes = await geometryTransformation.DirectTransformation(x_list, y_list, Srid, 25833);

        double minX = TransformedCoordianes.Xs.Min(); 
        double maxX = TransformedCoordianes.Xs.Max();
        double minY = TransformedCoordianes.Ys.Min();
        double maxY = TransformedCoordianes.Ys.Max();

        Envelope envelope = new Envelope(minX, maxX, minY, maxY);

        NetTopologySuite.Geometries.Geometry boundingBox = geometryFactoryTarget.ToGeometry(envelope);

        NetTopologySuite.Geometries.Prepared.IPreparedGeometry PreparedBoundingBox = PreparedGeometryFactory.Prepare(boundingBox);

        await using var transaction = context.Database.BeginTransaction();

        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var BoundingBoxIntersection = context.GeoData.
        Where(gd => gd.ParameterKey != landParcelID && gd.Geom!.Intersects(PreparedBoundingBox.Geometry)).
        Select(gd => new
        {
            gd.Geom,
            gd.Id,
            gd.ParameterKey,
            gd.Parameter
        });

        List<GeometryElementParameter> BoundingBoxIntersectionWithType = BoundingBoxIntersection
            .Join(
            context.GeothermalParameter,
            bb => bb.ParameterKey,
            gp => gp.Id,
            (bb, gp) => new GeometryElementParameter
            {
                Type = gp.Type,
                ParameterKey = bb.ParameterKey,
                Parameter = bb.Parameter,
                Geometry = bb.Geom
            }).ToList();

        context.SaveChanges();
        transaction.Commit();

        //Tree

        string? geoJsonTree = null;
        NetTopologySuite.Geometries.Geometry? unifiedGeometry = null;

        var allTreeData = BoundingBoxIntersectionWithType
        .Where(element => element.Type == TypeOfData.tree_vector)
        .ToList();

        var treeGeometries = allTreeData
        .Select(e => e.Geometry)
        .Where(g => g != null)
        .ToList();

        var geometryCollection = geometryFactoryTarget.CreateGeometryCollection(treeGeometries.ToArray());

        unifiedGeometry = geometryCollection.Union();

        geoJsonTree = geoJsonWriter.Write(unifiedGeometry);

        //Export Class

        var BoundingBoxresult = new BoundingBoxData
        {
            Treedata = new TreeData
            {
                Srid = unifiedGeometry?.SRID,
                GeojsonTree = geoJsonTree,
            },
        };

        return BoundingBoxresult;
    }
}