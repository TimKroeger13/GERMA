using GERMAG.Shared.PointProperties;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using NetTopologySuite.Geometries.Prepared;

namespace GERMAG.Server.GeometryCalculations;

public interface IThermalConductivity
{
    double? GetThermalConductivity(double? measuredConductivity, double? drillingDepth);
}

public class ThermalConductivity() : IThermalConductivity
{
    public double? GetThermalConductivity(double? measuredConductivity, double? drillingDepth)
    {
        var DepthBoarder = 100; //Depth to which to use the mesured Conductivity

        if (drillingDepth <= DepthBoarder)
        {
            return measuredConductivity;
        }

        var ExtendedDepth = drillingDepth - DepthBoarder;

        return (measuredConductivity * (DepthBoarder / (ExtendedDepth + DepthBoarder)) + OfficalParameters.ConductivityDeeperThan100Meter * (ExtendedDepth / (ExtendedDepth + DepthBoarder)));

    }
}