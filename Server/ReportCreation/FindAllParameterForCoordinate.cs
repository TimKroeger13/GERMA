using NetTopologySuite.Geometries;
using NetTopologySuite;
using GERMAG.DataModel.Database;
using GERMAG.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries.Utilities;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.IO.CoordinateSystems;
using NetTopologySuite.CoordinateSystems.Transformations;
using GeoAPI;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using NetTopologySuite.IO;

namespace GERMAG.Server.ReportCreation;

public interface IFindAllParameterForCoordinate
{
    List<GeometryElementParameter> FindCoordianteParameters(LandParcel landParcelElement, Restricion RestrictionFile, bool useLandParcel);
}

public class FindAllParameterForCoordinate(DataContext context) : IFindAllParameterForCoordinate
{
    public List<GeometryElementParameter> FindCoordianteParameters(LandParcel landParcelElement, Restricion RestrictionFile, bool useLandParcel)
    {
        var IntersectingGeometry = context.GeoData
            .Where(gd => gd.ParameterKey != landParcelElement.ParameterKey && gd.Geom!.Intersects(RestrictionFile.Geometry_Usable))
            .Select(gd => new
            {
                gd.ParameterKey,
                gd.Parameter
            });
        var landParcelResult = new GeometryElementParameter
        {
            Type = TypeOfData.land_parcels,
            ParameterKey = landParcelElement.ParameterKey,
            Parameter = landParcelElement.Parameter
        };

        var intersectingResult = IntersectingGeometry
            .Join(
                context.GeothermalParameter,
                ig => ig.ParameterKey,
                gp => gp.Id,
                (ig, gp) => new GeometryElementParameter
                {
                    Type = gp.Type,
                    ParameterKey = ig.ParameterKey,
                    Parameter = ig.Parameter
                })
            .ToList();

        List<GeometryElementParameter> result;

        if (useLandParcel)
        {
            result = intersectingResult.Append(landParcelResult).ToList();
        }
        else
        {
            result = intersectingResult.ToList();
        }

        return result;
    }
}