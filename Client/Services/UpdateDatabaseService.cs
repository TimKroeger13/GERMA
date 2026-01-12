using GERMAG.Client.Core;
using GERMAG.Shared.Core;

namespace GERMAG.Client.Services;

public interface IUpdateDatabaseService
{
    Task<string> greetFunction();
    Task<string> checkForUpdates();
}

public class UpdateDatabaseService(IRestInteropFactory factory) : IUpdateDatabaseService
{
    private readonly IRestInterop _restInterop = factory.CreateRestInterop("api/UpdateDatabase");

    public async Task<string> greetFunction()
    {
        return await _restInterop.GetAsync("greet") ?? "Server not reached";
    }

    public async Task<string> checkForUpdates()
    {
        return await _restInterop.GetAsync("checkForUpdates") ?? "Server not reached";
    }
}
