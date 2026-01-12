using GERMAG.DataModel.Database;
using GERMAG.Server.ExtensionMethods;
using GERMAG.Shared;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Simplify;
using NetTopologySuite.Utilities;

namespace GERMAG.Server.GeometryCalculations;

public interface IRestrictionFromLandParcel
{
    Task<Restricion> CalculateRestrictions(LandParcel landParcelElement, RequestContext request_context);
}

public class RestrictionFromLandParcel(DataContext context, IGeometryFromGeoJson geometryFromGeoJson) : IRestrictionFromLandParcel
{
    public async Task<Restricion> CalculateRestrictions(LandParcel landParcelElement, RequestContext request_context)
    {
        GeometryFactory geometryFactory = new();

        var geoJsonWriter = new GeoJsonWriter();

        //buildings1

        var buildingID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.building_surfaces).Id;

        var buldingIntersection = context.GeoData.Where(gd => gd.ParameterKey == buildingID && gd.Geom!.Intersects(landParcelElement.Geometry)).Select(gd => new { gd.Geom });

        //Trees

        NetTopologySuite.Geometries.Geometry? mergedTrees;

        if (request_context.UseTrees ?? true)
        {

            var TreeID = context.GeothermalParameter.Where(gp => gp.Type == TypeOfData.tree_vector).Select(gp => gp.Id).ToList();

            var treeIntersection = context.GeoData.Where(gd => TreeID.Contains(gd.ParameterKey) && gd.Geom!.Intersects(landParcelElement.Geometry)).Select(gd => new { gd.Geom });

            mergedTrees = new GeometryFactory().BuildGeometry(treeIntersection.Select(item => item.Geom)).Union();

            if (request_context.TreeBuffer != 0)
            {
                mergedTrees = mergedTrees?.Buffer(request_context.TreeBuffer ?? 0);
            }
        }
        else
        {
            mergedTrees = geometryFactory.CreatePolygon();
        }

        //Landpacel

        NetTopologySuite.Geometries.Geometry landParcelGeometry = landParcelElement.Geometry ?? geometryFactory.CreatePolygon();

        List<LineString> exteriorRings = new List<LineString>(); //Here

        if(landParcelGeometry is Polygon singlePolygon)
        {
            exteriorRings.Add(singlePolygon.ExteriorRing);
        }
        else if(landParcelGeometry is MultiPolygon multiPolygon)
        {
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                var subPolygon = multiPolygon.GetGeometryN(i) as Polygon;
                if(subPolygon != null)
                {
                    exteriorRings.Add(subPolygon.ExteriorRing);
                }
            }
        }

        //LineString landParcelLineString = Ex
        
        //NetTopologySuite.Geometries.Geometry? bufferedLandParcel2 = landParcelLineString?.Buffer(request_context.LandParcelDistance ?? OfficalParameters.LandParcelDistance); // + OfficalParameters.LandParcelDistance

        double bufferDistance = request_context.LandParcelDistance ?? OfficalParameters.LandParcelDistance;
        List<NetTopologySuite.Geometries.Geometry> bufferedGeometries = exteriorRings
            .Select(ring => ring.Buffer(bufferDistance))
            .Where(g => !g.IsEmpty)
            .ToList();


        NetTopologySuite.Geometries.Geometry? bufferedLandParcel = bufferedGeometries.Count switch
        {
            0 => null,
            1 => bufferedGeometries[0],
            _ => geometryFactory.BuildGeometry(bufferedGeometries).Union()
        };

        //buildings2

        NetTopologySuite.Geometries.Geometry? bufferedBuldings;

        if (request_context.UseBuildings ?? throw new Exception("CE: UseBuildings are for some strange reason null that should not be possible!!!"))
        {

            //buildings and parcel distance
            NetTopologySuite.Geometries.Geometry? mergedBuildings = new GeometryFactory().BuildGeometry(buldingIntersection.Select(item => item.Geom)).Union();

            if (mergedBuildings?.IsEmpty != false)
            {
                mergedBuildings = geometryFactory.CreatePolygon();
            }

            bufferedBuldings = mergedBuildings?.Buffer(OfficalParameters.BuildingDistance); // + (OfficalParameters.ProbeDiameter / 2)

        }
        else
        {
            bufferedBuldings = geometryFactory.CreateGeometryCollection(null);
        }

        //Validation of the Landpacel Polygon

        NetTopologySuite.Geometries.Geometry? UsableArea;

        if (!landParcelGeometry!.IsValid)
        {
            landParcelGeometry = (Polygon)TopologyPreservingSimplifier.Simplify(landParcelGeometry, 0.05);
        }
        if (!landParcelGeometry.IsValid)
        {
            landParcelGeometry = (Polygon)landParcelGeometry.Buffer(0);
        }

        //Protection Areas

        NetTopologySuite.Geometries.Geometry? MergedProtectionArea;

        var ProtectionId = context.GeothermalParameter.Where(gp => gp.Type == TypeOfData.protection_area).Select(gp => gp.Id).ToList();

        var ProtectionIntersection = context.GeoData.Where(gd => ProtectionId.Contains(gd.ParameterKey) && gd.Geom!.Intersects(landParcelElement.Geometry)).Select(gd => new { gd.Geom });

        MergedProtectionArea = new GeometryFactory().BuildGeometry(ProtectionIntersection.Select(item => item.Geom)).Union();

        //Custom Geometry Polygons

        var GeometryFactoryFactory = NetTopologySuite.Geometries.GeometryFactory.Default;

        List<LandParcel>? ListOfReprojectedNegativeAreas = await geometryFromGeoJson.GetPolygonsFromGeoJson(request_context.GeojsonMinus ?? "", request_context.Srid ?? 0);

        NetTopologySuite.Geometries.Geometry? MergeListOfReprojectedNegativArea = GeometryFactoryFactory.CreateGeometryCollection(null);

        if (ListOfReprojectedNegativeAreas != null)
        {
            MergeListOfReprojectedNegativArea = new GeometryFactory().BuildGeometry(ListOfReprojectedNegativeAreas.Select(i => i.Geometry)).Union();
        }

        //Custom Superior Polygons

        var GeometryFactoryFactoryFactory = NetTopologySuite.Geometries.GeometryFactory.Default;

        List<LandParcel>? ListOfReprojectedSuperiorAreas = await geometryFromGeoJson.GetPolygonsFromGeoJson(request_context.GeojsonSuperior ?? "", request_context.Srid ?? 0);

        NetTopologySuite.Geometries.Geometry? MergeListOfReprojectedSuperiorArea = GeometryFactoryFactoryFactory.CreateGeometryCollection(null);

        if (ListOfReprojectedSuperiorAreas != null)
        {
            MergeListOfReprojectedSuperiorArea = new GeometryFactory().BuildGeometry(ListOfReprojectedSuperiorAreas.Select(i => i.Geometry)).Union();
        }

        //Create Usable area

        UsableArea = landParcelGeometry?.Difference(bufferedLandParcel).Difference(bufferedBuldings).Difference(mergedTrees).Difference(MergedProtectionArea).Difference(MergeListOfReprojectedNegativArea);
        UsableArea = UsableArea?.Union(MergeListOfReprojectedSuperiorArea);

        //Validate Usable Areas

        var validPolygons = new List<NetTopologySuite.Geometries.Polygon>();
        NetTopologySuite.Geometries.Geometry? RecutUsableArea = null;

        if (UsableArea is Polygon polygon)
        {
            if (polygon.Area >= OfficalParameters.MinimalAreaSize)
            {
                RecutUsableArea = polygon;
            }

        }
        else if (UsableArea is MultiPolygon multipolygon)
        {
            foreach (Polygon p in multipolygon)
            {
                if (p.Area >= OfficalParameters.MinimalAreaSize)
                {
                    validPolygons.Add(p);
                }
            }

            if (validPolygons.Count == 1)
            {
                RecutUsableArea = validPolygons[0];
            }
            else if (validPolygons.Count > 1)
            {
                RecutUsableArea = geometryFactory.CreateMultiPolygon(validPolygons.ToArray());
            }
        }

        if (RecutUsableArea == null)
        {

            var FaliedReturn = new Restricion
            {
                Geometry_Usable = landParcelElement.Geometry,
                Geometry_Restiction = null,
                Geometry_Usable_geoJson = null,
                Geometry_Restiction_geoJson = null,
                Usable_Area = null,
                Restiction_Area = null,
            };

            return FaliedReturn;
        }

        //Union of usable area

        UsableArea = RecutUsableArea.Union();

        //Restriction areaZY

        NetTopologySuite.Geometries.Geometry? RestictionArea;

        RestictionArea = bufferedLandParcel?
            .Union(bufferedBuldings)
            .Union(mergedTrees)
            .Union(MergedProtectionArea)
            .Union(MergeListOfReprojectedNegativArea);

        RestictionArea = RestictionArea?.Difference(MergeListOfReprojectedSuperiorArea);

        RestictionArea = landParcelGeometry?.Intersection(RestictionArea);
        RestictionArea = RestictionArea?.Union();

        //Return

        var returnValue = new Restricion
        {
            Geometry_Usable = UsableArea,
            Geometry_Restiction = RestictionArea,
            Geometry_Usable_geoJson = geoJsonWriter.Write(UsableArea),
            Geometry_Restiction_geoJson = geoJsonWriter.Write(RestictionArea),
            Usable_Area = UsableArea?.Area ?? 0,
            Restiction_Area = RestictionArea?.Area ?? 0,
        };

        return returnValue;
    }
}