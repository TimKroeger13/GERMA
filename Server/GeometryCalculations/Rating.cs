using GERMAG.Shared;

namespace GERMAG.Server.GeometryCalculations;

public interface IRating
{
    Task<decimal?> CalculateRating(double? depth, double? con, double? temp, bool? waterProtect);
}

public class Rating : IRating
{
    public async Task<decimal?> CalculateRating(double? depth, double? con, double? temp, bool? waterProtect)
    {
        var ValueTreshold = Treshold(depth, con, temp, waterProtect);

        if (!ValueTreshold) { return 0; }

        var ValueDepthFactor = await DepthFactor(depth);
        var ValueThermalConFactor = await ThermalConFactor(con);
        var VlaueUnderGroundTempFactor = await UnderGroundTempFactor(temp);

        var RatinValue = ((ValueDepthFactor * OfficalParameters.DepthFactorRatio) +
            (ValueThermalConFactor * OfficalParameters.ThermalConFactorRatio) +
            (VlaueUnderGroundTempFactor * OfficalParameters.UnderGroundTempFactorRatio));

        return Math.Round((decimal)RatinValue,2);
    }

    public bool Treshold(double? depth, double? con, double? temp, bool? waterProtect)
    {
        if (waterProtect == true) { return false; }
        if (depth < OfficalParameters.DepthFactorMin) { return false; }
        if (temp < OfficalParameters.UnderGroundTempMin) { return false; }
        if (con < OfficalParameters.ThermalConFactorMin) { return false; }
        return true;
    }

    public async Task<double?> DepthFactor(double? depth)
    {
        return await Task.Run(() =>
        {
            if (depth < OfficalParameters.DepthFactorMin) { return 0; }
            if (depth > OfficalParameters.DepthFactorMax) { return 10; }

            return ((depth - 30) / 7);
        });
    }

    public async Task<double?> ThermalConFactor(double? con)
    {
        return await Task.Run(() =>
        {
            if (con == null) { return 0; }
            if (con < OfficalParameters.ThermalConFactorMin) { return 0; }
            if (con > OfficalParameters.ThermalConFactorMax) { return 10; }

            return (6 * Math.Log((double)(con - 1.320569)) + 7.65);
        });
    }

    public async Task<double?> UnderGroundTempFactor(double? temp)
    {
        return await Task.Run(() =>
        {
            if (temp < OfficalParameters.UnderGroundTempMin) { return 0; }
            if (temp > OfficalParameters.UnderGroundTempMax) { return 10; }

            return ((64 * (temp - 8)) / (-temp + 45));
        });
    }
}