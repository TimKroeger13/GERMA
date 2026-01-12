using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public static class OfficalParameters
{
    public static double ProbeDiameter { get; } = 0; // meter (radius)
    public static double TreeDistance { get; } = 4; // meter (radius)
    public static double LandParcelDistance { get; } = 3; // meter 0 für die AUswertung
    public static double BuildingDistance { get; } = 2; // meter
    public static double ProbeDistance { get; } = 6; //meter (radius)
    public static int MaximalAreaSizeForCalculations { get; } = 200000; //sqaure meters 100000

    public static int FastCalculationThreshhold { get; } = 10000; //square meters
    public static double MinimalAreaSize { get; } = (ProbeDistance * (ProbeDistance * Math.Sin(((double)60 / (double)180 * Math.PI)))) / 2; //~15.88

    public static double DepthFactorRatio { get; } = (double)24.74 / (double)100;
    public static double ThermalConFactorRatio { get; } = (double)18.26 / (double)100;
    public static double UnderGroundTempFactorRatio { get; } = (double)57 / (double)100;

    public static double DepthFactorMin { get; } = 30;
    public static double DepthFactorMax { get; } = 400;
    public static double LimitedDepthFactorMax { get; } = 100;
    public static double ThermalConFactorMin { get; } = 1.6;
    public static double ThermalConFactorMax { get; } = 2.8;
    public static double UnderGroundTempMin { get; } = 8;
    public static double UnderGroundTempMax { get; } = 13;
    public static double ConductivityDeeperThan100Meter { get; } = 2.4;
    public static double FixCOP { get; } = 3;
    public static double DepthChunkValue { get; } = 5; //m
    public static double ShallowChunkValue { get; } = 1; //m

    public static int VollLoadhours { get; } = 2400; //h/a
}
