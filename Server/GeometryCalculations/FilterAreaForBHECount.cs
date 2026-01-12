using GeoAPI.Geometries;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Buffer;

public interface IFilterAreaForBHECount
{
    NetTopologySuite.Geometries.Geometry? GetFilteredArea(NetTopologySuite.Geometries.Geometry? geometry, RequestContext request_context);
}

public class FilterAreaForBHECount() : IFilterAreaForBHECount
{
    static readonly GeometryFactory geometryFactory = new GeometryFactory();
    public NetTopologySuite.Geometries.Geometry? GetFilteredArea(NetTopologySuite.Geometries.Geometry? geometry, RequestContext request_context)
    {
        if (geometry == null) { return geometry; }

        var geomFactory = geometry.Factory;

        // Normalize to MultiPolygon for simplicity
        MultiPolygon multiPolygon = geometry switch
        {
            Polygon p => geomFactory.CreateMultiPolygon([p]),
            MultiPolygon mp => mp,
            _ => throw new ArgumentException("Input geometry must be a Polygon or MultiPolygon")
        };

        // Extract and sort sub-polygons by area (smallest first)
        var sortedPolygons = new List<Polygon>();
        for (int i = 0; i < multiPolygon.NumGeometries; i++)
        {
            var sub = (Polygon)multiPolygon.GetGeometryN(i);
            sortedPolygons.Add(sub);
        }

        sortedPolygons = sortedPolygons.OrderBy(p => p.Area).ToList();

        var filtered = new List<Polygon>();

        var skipChecks = false;

        for (int i = 0; i < sortedPolygons.Count; i++)
        {

            var polygon = sortedPolygons[i];

            if (skipChecks)
            {
                filtered.Add(polygon);
                continue;
            }
            // Always keep the largest one (i.e., last in the list)
            bool isLast = (i == sortedPolygons.Count - 1);

            if (isLast)
            {
                filtered.Add(polygon);
                continue;
            }

            // Placeholder condition for small useful polygons
            bool isSmallUseful = polygon.Area < 20.0; // square meters

            var (GeometryFits, EarlyCutOff) = CheckMinimalBHEAmount(polygon, request_context);

            if (EarlyCutOff)
            {
                skipChecks = true;
            }

            if (isLast || GeometryFits)
                filtered.Add(polygon);
        }

        if (filtered.Count == 1)
        {
            return filtered[0];
        }

        return geomFactory.CreateMultiPolygon(filtered.ToArray());

    }


    private (bool GeometryFits, bool EarlyCutOff) CheckMinimalBHEAmount(NetTopologySuite.Geometries.Polygon geom, RequestContext request_context)
    {
        int EarlyCutOffSavetyValue = (int)Math.Ceiling((double)request_context.MinimalBhePerInduvidualArea! * 1.5);

        var CurrentLoopCounter = 0;

        var geoJsonWriter = new GeoJsonWriter();
        int CurrentSubpolygonGroupNumber = 0;

        NetTopologySuite.Geometries.Geometry? currentGeometry;
        NetTopologySuite.Geometries.Geometry? currentOutline;
        NetTopologySuite.Geometries.Coordinate[]? currentPoints;
        double? currentArea;
        List<NetTopologySuite.Geometries.Coordinate?> CandidatePoints = [];
        NetTopologySuite.Geometries.MultiPoint? CandidateMultiPoint;
        NetTopologySuite.Geometries.Geometry? CandidateBufferRing;
        NetTopologySuite.Geometries.Point? lastCurrentPoint;
        ProbePoint? CandidateChoosenPoint;
        NetTopologySuite.Geometries.Geometry? smallestAreaBuffer = null;
        int smallestAreaIndex;
        double[] distances;
        int indexOfCandidate;

        List<ProbePoint?> ReportGeothermalPoints = [];
        List<ProbePoint?> InitalFastCalculationPoints = [];

        currentGeometry = geom;
        currentOutline = geom?.Boundary;
        currentPoints = geom?.Coordinates;
        currentArea = geom?.Area;

        var centroid = geom?.Centroid;

        if (currentArea == 0)
        {
            return (false, false);
        }

        distances = new double[currentPoints!.Length];

        for (int i = 0; i < currentPoints?.Length; i++)
        {
            distances[i] = centroid?.Distance(new NetTopologySuite.Geometries.Point(currentPoints[i])) ?? 0;
        }

        indexOfCandidate = Array.IndexOf(distances, distances.Max());

        if (indexOfCandidate != -1)
        {
            CandidatePoints.Add(currentPoints?[indexOfCandidate]);
        }

        if (CandidatePoints.Count == 0)
        {
            return (false, false);
        }

        while (geom!.Area > 0 && CurrentLoopCounter < EarlyCutOffSavetyValue)
        {
            CurrentLoopCounter++;

            //Console.WriteLine(currentArea);
            CandidateMultiPoint = geometryFactory.CreateMultiPointFromCoords(CandidatePoints.ToArray());


            NetTopologySuite.Geometries.Polygon? candidateBuffer;
            var combinedIntersectionCollection = new List<NetTopologySuite.Geometries.Geometry?>(CandidateMultiPoint.NumGeometries);
            var combinedBufferCollection = new List<NetTopologySuite.Geometries.Geometry?>(CandidateMultiPoint.NumGeometries);

            foreach (var TempCandidatePoint in CandidateMultiPoint.Geometries)
            {
                candidateBuffer = (NetTopologySuite.Geometries.Polygon)TempCandidatePoint.Buffer(request_context.CustomProbeDistance ?? OfficalParameters.ProbeDistance + (OfficalParameters.ProbeDiameter));

                var TempBufferIntersectionSingle = currentGeometry?.Intersection(candidateBuffer);

                if (TempBufferIntersectionSingle != null)
                {
                    combinedIntersectionCollection.Add(TempBufferIntersectionSingle);
                    combinedBufferCollection.Add(candidateBuffer);
                }
            }

            smallestAreaIndex = -1;
            double smallestArea = double.MaxValue;

            for (int i = 0; i < combinedIntersectionCollection.Count; i++)
            {
                var candidateGeometry = combinedIntersectionCollection[i];

                if (candidateGeometry is not null)
                {
                    if (!candidateGeometry.IsValid)
                    {
                        continue;
                    }

                    if (candidateGeometry is not MultiPolygon)
                    {
                        if (candidateGeometry is GeometryCollection geometryCollection)
                        {
                            var polygonsInGeometryCollection = new List<Polygon>(geometryCollection.NumGeometries);

                            foreach (var Geometry in geometryCollection)
                            {
                                if (Geometry is Polygon localPolygon)
                                {
                                    polygonsInGeometryCollection.Add(localPolygon);
                                }
                            }
                            candidateGeometry = new NetTopologySuite.Geometries.MultiPolygon(polygonsInGeometryCollection.ToArray());
                        }
                    }

                    if (candidateGeometry is MultiPolygon multiPolygon)
                    {
                        double totalArea = 0.0;

                        foreach (var polygon in multiPolygon)
                        {
                            totalArea += polygon.Area;
                        }

                        if (totalArea < smallestArea)
                        {
                            smallestArea = totalArea;
                            smallestAreaBuffer = combinedBufferCollection[i];
                            smallestAreaIndex = i;
                        }
                    }
                    else if (candidateGeometry is NetTopologySuite.Geometries.Polygon polygon)
                    {
                        double minArea = polygon.Area;

                        if (minArea < smallestArea)
                        {
                            smallestArea = minArea;
                            smallestAreaBuffer = combinedBufferCollection[i];
                            smallestAreaIndex = i;
                        }
                    }
                }
            }

            if (smallestAreaBuffer!.IsValid)
            {
                lastCurrentPoint = new GeometryFactory().CreatePoint(CandidatePoints[smallestAreaIndex]);


                CandidateChoosenPoint = new()
                {
                    Geometry = lastCurrentPoint,
                    GeometryJson = geoJsonWriter.Write(lastCurrentPoint),
                    Properties = new GERMAG.Shared.PointProperties.Properties { GeoPoten = null, MaxDepth = null, GeoPotenDepth = null, Extraction_KW_2400_Full_Load_Hours = null, Rating = null, SubAreaFieldNumber = CurrentSubpolygonGroupNumber }
                };

                ReportGeothermalPoints.Add(CandidateChoosenPoint);

                CandidatePoints.Clear();

                //Find Candiate Points in nearby geometry

                var precision = new BufferParameters
                {
                    QuadrantSegments = 8 // try 1–4 for lower resolution, 8 is default
                };

                CandidateBufferRing = lastCurrentPoint.Buffer(request_context.CustomProbeDistance ?? OfficalParameters.ProbeDistance + (OfficalParameters.ProbeDiameter), precision).Boundary;

                if (currentOutline is NetTopologySuite.Geometries.MultiLineString multiLineString)
                {
                    foreach (var lineString in multiLineString.Geometries)
                    {
                        if (lineString is NetTopologySuite.Geometries.LinearRing && CandidateBufferRing is NetTopologySuite.Geometries.LinearRing)
                        {
                            CandidatePoints = FindNewCandidates(lineString, CandidateBufferRing, CandidatePoints);
                        }
                    }
                }
                else if (currentOutline is NetTopologySuite.Geometries.LinearRing SoloLineString)
                {
                    CandidatePoints = FindNewCandidates(SoloLineString, CandidateBufferRing, CandidatePoints);
                }

                //Update Data

                currentGeometry = currentGeometry?.Difference(smallestAreaBuffer);

                if (currentGeometry is MultiPolygon multiPolygon)
                {
                    List<Polygon> validPolygons = new();

                    foreach (var polygon in multiPolygon.Geometries.OfType<Polygon>())
                    {
                        double polygonArea = polygon.Area;

                        if (polygonArea >= 1)
                        {
                            validPolygons.Add(polygon);
                        }
                    }

                    currentGeometry = validPolygons.Count > 0 ? new MultiPolygon(validPolygons.ToArray()) : null;
                }
                else if (currentGeometry is Polygon polygon)
                {
                    double polygonArea = polygon.Area;

                    if (polygonArea < 1)
                    {
                        currentGeometry = null;
                    }
                }
                if (currentGeometry is Polygon || currentGeometry is MultiPolygon)
                {
                    currentOutline = currentGeometry?.Boundary;
                    currentPoints = currentGeometry?.Coordinates;
                    currentArea = currentGeometry?.Area;
                }
                else
                {
                    currentOutline = null;
                    currentPoints = null;
                    currentArea = 0;
                }
            }
            else
            {
                CandidatePoints.Clear();
            }

            if (currentArea == 0 || currentPoints == null)
            {
                return (ReportGeothermalPoints.Count >= request_context.MinimalBhePerInduvidualArea, ReportGeothermalPoints.Count >= EarlyCutOffSavetyValue - 1);

            }

            if (CandidatePoints.Count == 0)
            {
                double maxDistance = double.MinValue;
                indexOfCandidate = -1;

                for (int i = 0; i < currentPoints?.Length; i++)
                {
                    double dist = centroid?.Distance(new Point(currentPoints[i])) ?? 0;
                    if (dist > maxDistance)
                    {
                        maxDistance = dist;
                        indexOfCandidate = i;
                    }
                }


                if (indexOfCandidate != -1)
                {
                    CandidatePoints.Add(currentPoints?[indexOfCandidate]);
                }

                if (CandidatePoints.Count == 0 || currentPoints == null)
                {
                    return (ReportGeothermalPoints.Count >= request_context.MinimalBhePerInduvidualArea, ReportGeothermalPoints.Count >= EarlyCutOffSavetyValue - 1);
                }
            }
        }

        return (ReportGeothermalPoints.Count >= request_context.MinimalBhePerInduvidualArea, ReportGeothermalPoints.Count >= EarlyCutOffSavetyValue - 1);
    }


    private List<NetTopologySuite.Geometries.Coordinate?> FindNewCandidates(NetTopologySuite.Geometries.Geometry SearchLineRing, NetTopologySuite.Geometries.Geometry CandidateBufferRing, List<NetTopologySuite.Geometries.Coordinate?> CandidatePoints)
    {
        var lineStringIntersection = CandidateBufferRing?.Intersection(SearchLineRing);

        if (lineStringIntersection is NetTopologySuite.Geometries.Point point)
        {
            CandidatePoints.Add(point.Coordinate);
        }
        else if (lineStringIntersection is NetTopologySuite.Geometries.MultiPoint multiPoint)
        {
            CandidatePoints.AddRange(multiPoint.Coordinates);
        }
        else if (lineStringIntersection is NetTopologySuite.Geometries.GeometryCollection geometryCollection)
        {
            foreach (var geometry in geometryCollection.Geometries)
            {
                if (geometry is NetTopologySuite.Geometries.Point collectionPoint)
                {
                    CandidatePoints.Add(collectionPoint.Coordinate);
                }
            }
        }

        return CandidatePoints;
    }
}