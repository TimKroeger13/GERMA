using GERMAG.Shared.Core;
namespace GERMAG.Client.Core;

public interface IRestInteropFactory
{
    IRestInterop CreateRestInterop(string baseRoute);
}

public class RestInteropFactory(HttpClient httpClient, IErrorLogger logger) : IRestInteropFactory
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IErrorLogger _logger = logger;

    public IRestInterop CreateRestInterop(string baseRoute)
    {
        return new RestInterop(baseRoute, _httpClient, _logger);
    }
}
