using GERMAG.Shared.Core;
using GERMAG.Client.Core;

namespace GERMAG.Client.Services;

public interface ICalcualteResearchParameterService
{
    Task<string> calcualteResearch();
}

public class CalcualteResearchParameterService(IRestInteropFactory factory) : ICalcualteResearchParameterService
{
    private readonly IRestInterop _restInterop = factory.CreateRestInterop("api/Research");
    public async Task<string> calcualteResearch()
    {
        return await _restInterop.GetAsync("research") ?? "Server not reached";
    }
}
