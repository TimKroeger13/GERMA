using GERMAG.Shared.PointProperties;

namespace GERMAG.Server.GeometryCalculations;

public interface ICrossInfluence
{
    Task<double> GetCrossInfluenceFactor(int? NumberOfProbePoints);
}

public class CrossInfluence : ICrossInfluence
{
    public async Task<double> GetCrossInfluenceFactor(int? NumberOfProbePoints)
    {
        return await Task.Run(() =>
        {
            int? n = NumberOfProbePoints;
            double CrossFactor;

            if (n == null)
            {
                throw new Exception("CE: Number of Probe Points not defined!");
            }

            if (n == 1)
            {
                CrossFactor = 1.1;
            }
            else if (n < 70)
            {
                CrossFactor = (159.863 - 12.5247 * Math.Log((double)(100.761 * n - 141.675))) / 100;
            }
            else
            {
                CrossFactor = 0.49;
            }

            return CrossFactor;
        });
    }
}
