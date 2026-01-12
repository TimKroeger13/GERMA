using GeoAPI.Geometries;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Simplify;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeoThermalProbesCalcualtion
{
    Task<List<ProbePoint?>> CalculateGeoThermalProbes(Restricion RestrictionAreas, RequestContext request_context, TemplateBHE? templateBHE);
}

public class GeoThermalProbesCalcualtion(IFilterAreaForBHECount filterAreaForBHECount) : IGeoThermalProbesCalcualtion
{
    static readonly GeometryFactory geometryFactory = new GeometryFactory();
    public async Task<List<ProbePoint?>> CalculateGeoThermalProbes(Restricion RestrictionAreas, RequestContext request_context, TemplateBHE? templateBHE)
    {
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

        NetTopologySuite.Geometries.Geometry? usableArea = RestrictionAreas?.Geometry_Usable;

        //Get fix BHE count when wanted

        int BHECounter = 0;
        int MaximumBHECount = int.MaxValue;

        if (!request_context.CalculateMaximalFieldSize ?? false)
        {
            MaximumBHECount = templateBHE!.BHECount ?? throw new Exception("CE: No BHE Count exsist");
        }

        //check for to Large Areas

        if (usableArea?.Area > OfficalParameters.MaximalAreaSizeForCalculations)
        { throw new Exception("Selected Area is to Large"); }

        //update Geometry

        usableArea = usableArea?.Buffer(-(OfficalParameters.ProbeDiameter / 2));

        //calculate SubAreas
        List<(int i, Polygon p)> ListOfSubpolygons = [];
        if (usableArea is MultiPolygon UsableAreaTotalMultipolyon)
        {
            var i = 0;
            foreach (Polygon UsableAreaSubPolygon in UsableAreaTotalMultipolyon.Cast<Polygon>())
            {
                i++;
                ListOfSubpolygons.Add((i, UsableAreaSubPolygon));
            }
        }
        if (usableArea is Polygon UsableAreaTotalPolygon)
        {
            ListOfSubpolygons.Add((1, UsableAreaTotalPolygon));
        }



        //Fast Probepoint Calculation
        //input:
        

        //Filter here out to Samll polygons


        //////////////////////////////////////////////
        //////////////////////////////////////////////

        if (usableArea is NetTopologySuite.Geometries.MultiPolygon)
        {
            usableArea = filterAreaForBHECount.GetFilteredArea(usableArea, request_context);
        }
        //////////////////////////////////////////////
        //////////////////////////////////////////////
        
        if (!usableArea!.IsValid)
        {
            usableArea = (Polygon)TopologyPreservingSimplifier.Simplify(usableArea, 0.05);
        }
        if (!usableArea.IsValid)
        {
            usableArea = (Polygon)usableArea.Buffer(0);
        }


        //ListOfSubpolygons
        //request_context.CustomProbeDistance

        //if (usableArea?.Area > OfficalParameters.FastCalculationThreshhold && (request_context.CalculateMaximalFieldSize ?? false))
        if (usableArea?.Area > OfficalParameters.FastCalculationThreshhold && MaximumBHECount != 0)
        {

            var envelope = usableArea.Envelope;
            var TL_x = envelope.Envelope.Coordinates[1].X;
            var TL_y = envelope.Envelope.Coordinates[1].Y;
            var BR_x = envelope.Envelope.Coordinates[3].X;
            var BR_y = envelope.Envelope.Coordinates[3].Y;

            var enevelopeWidth = BR_x - TL_x;
            var enevelopeHight = TL_y - BR_y;

            var Probe_x_distance = request_context.CustomProbeDistance;
            var Probe_y_distance = Math.Sqrt(Math.Pow(Probe_x_distance ?? 0, 2) - Math.Pow((Probe_x_distance / 2) ?? 0, 2));

            var X_Probe_Cycles = Math.Floor((decimal)(enevelopeWidth / Probe_x_distance ?? 0)) + 1;
            var Y_Probe_Cycles = Math.Floor((decimal)(enevelopeHight / Probe_y_distance)) + 1;

            var base_x = TL_x;
            var base_y = BR_y;

            var PreciseGeometryFactory = new GeometryFactory(new PrecisionModel(), 25833);

            for (int h_cor = 0; h_cor < Y_Probe_Cycles; h_cor++)
            {
                for (int w_cor = 0; w_cor < X_Probe_Cycles; w_cor++)
                {
                    var x = base_x + (w_cor * Probe_x_distance);
                    var y = base_y + (h_cor * Probe_y_distance);

                    if (h_cor % 2 == 0)
                    {
                        x += (Probe_x_distance / 2);
                    }

                    // Create the point
                    Point point = PreciseGeometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(x ?? 0, y));

                    CurrentSubpolygonGroupNumber = ListOfSubpolygons.OrderBy(sub => sub.p.Distance(point)).First().i;

                    CandidateChoosenPoint = new()
                    {
                        Geometry = point,
                        GeometryJson = geoJsonWriter.Write(point),
                        Properties = new Shared.PointProperties.Properties { GeoPoten = null, MaxDepth = null, GeoPotenDepth = null, Extraction_KW_2400_Full_Load_Hours = null, Rating = null, SubAreaFieldNumber = CurrentSubpolygonGroupNumber, ThermalCon = templateBHE?.ThermalCon}
                    };

                    InitalFastCalculationPoints.Add(CandidateChoosenPoint);

                }
            }

            var prepUsableArea = PreparedGeometryFactory.Prepare(usableArea);

            var filteredPoints = InitalFastCalculationPoints
                .Where(p => prepUsableArea.Contains(p!.Geometry))
                .Take(MaximumBHECount)
                .ToList();

            BHECounter += filteredPoints.Count;

            var pointGeometries = filteredPoints.Select(pp => pp!.Geometry).Cast<NetTopologySuite.Geometries.Geometry>().ToList();

            ReportGeothermalPoints = filteredPoints;

            var geothermalBuffers = pointGeometries
                .Select(p => p.Buffer(request_context.CustomProbeDistance ?? throw new Exception("CE: CustomProbedistance not defined")))
                .ToList();

            var unionedBuffer = new GeometryFactory().BuildGeometry(geothermalBuffers).Union();

            usableArea = usableArea.Difference(unionedBuffer);

        }

        //Findet Left over smaler Areas

        var centroid = usableArea?.Centroid;

        //Inital cycle

        currentGeometry = usableArea;
        currentOutline = usableArea?.Boundary;
        currentPoints = usableArea?.Coordinates;
        currentArea = usableArea?.Area;

        if (currentArea == 0)
        {
            return ReportGeothermalPoints;
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
            return ReportGeothermalPoints;
        }



        //loop start

        while (currentArea > 0)
        {
            if (MaximumBHECount <= BHECounter) { break; }
            BHECounter++;

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

                CurrentSubpolygonGroupNumber = ListOfSubpolygons.OrderBy(sub => sub.p.Distance(lastCurrentPoint)).First().i;

                //Console.WriteLine(CurrentSubpolygonGroupNumber);

                CandidateChoosenPoint = new()
                {
                    Geometry = lastCurrentPoint,
                    GeometryJson = geoJsonWriter.Write(lastCurrentPoint),
                    Properties = new Shared.PointProperties.Properties { GeoPoten = null, MaxDepth = null, GeoPotenDepth = null, Extraction_KW_2400_Full_Load_Hours = null, Rating = null, SubAreaFieldNumber = CurrentSubpolygonGroupNumber , ThermalCon = templateBHE?.ThermalCon}
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
                            CandidatePoints = await FindNewCandidates(lineString, CandidateBufferRing, CandidatePoints);
                        }
                    }
                }
                else if (currentOutline is NetTopologySuite.Geometries.LinearRing SoloLineString)
                {
                    CandidatePoints = await FindNewCandidates(SoloLineString, CandidateBufferRing, CandidatePoints);
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
                return ReportGeothermalPoints;
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
                    return ReportGeothermalPoints;
                }
            }
        }

        return ReportGeothermalPoints;
    }

    private async Task<List<NetTopologySuite.Geometries.Coordinate?>> FindNewCandidates(NetTopologySuite.Geometries.Geometry SearchLineRing, NetTopologySuite.Geometries.Geometry CandidateBufferRing, List<NetTopologySuite.Geometries.Coordinate?> CandidatePoints)
    {
        return await Task.Run(() =>
        {
            NetTopologySuite.Geometries.Geometry? lineStringIntersection = null;

            try
            {
                lineStringIntersection = CandidateBufferRing.Intersection(SearchLineRing);
            }
            catch (TopologyException ex)
            {
                Console.WriteLine("Trying to fix TopologyException: " + ex.Message);

                // Attempt fallback: clean both with Buffer(0)
                var cleanA = TopologyPreservingSimplifier.Simplify(CandidateBufferRing, 0.05);
                cleanA = cleanA.Buffer(0);
                var cleanB = TopologyPreservingSimplifier.Simplify(SearchLineRing, 0.05);
                cleanB = cleanB.Buffer(0);

                try
                {
                    lineStringIntersection = cleanA.Intersection(cleanB);
                }
                catch (Exception ex2)
                {
                    throw new Exception("CE: non-noded intersection between linerings: " + ex2.Message);
                }
            }

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
        });
    }
}