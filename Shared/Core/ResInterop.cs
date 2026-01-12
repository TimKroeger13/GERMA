using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GERMAG.Shared.Core;

public interface IRestInterop
{
    Task DeleteAsync(string relativeUri);

    Task<string?> GetAsync(string relativeUri);

    Task<TReturn?> GetAsync<TReturn>(string relativeUri);

    Task PostAsync(string relativeUri);

    Task<string?> PostAsync<TRequest>(string relativeUri, TRequest request);

    Task<TReturn?> PostAsync<TReturn, TRequest>(string relativeUri, TRequest request);

    Task<string?> PostAsyncRaw<TRequest>(string relativeUri, TRequest request);

    Task PutAsync(string relativeUri);

    Task PutAsync<TRequest>(string relativeUri, TRequest request);

    Task<TReturn?> PutAsync<TReturn, TRequest>(string relativeUri, TRequest request);
}

public class RestInterop : IRestInterop
{
    protected readonly HttpClient HttpClient;
    private readonly IErrorLogger _logger;
    private readonly string _baseRoute;
    private static JsonSerializerOptions s_jsonOptions = null!;

    public RestInterop(string baseRoute, HttpClient http, IErrorLogger logger)
    {
        _baseRoute = baseRoute;
        s_jsonOptions ??= new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };
        HttpClient = http;
        _logger = logger;
    }

    private async Task FilterStatusCodes(HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
        {
            await _logger.LogError("Rights are not sufficient for the action");
        }
    }

    private string BuildUri(string relativeUri)
    {
        return $"{_baseRoute}/{relativeUri}".Replace("//", "/").Replace("/?", "?").TrimEnd('/');
    }

    public async Task<TReturn?> GetAsync<TReturn>(string relativeUri)
    {
        using var result = await HttpClient.GetAsync(BuildUri(relativeUri));
        if (result.IsSuccessStatusCode)
        {
            return await ReadFromJson<TReturn>(result);
        }
        else
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg ?? "Could not fetch from " + _baseRoute + "/" + relativeUri);
            return default;
        }
    }

    public async Task<string?> GetAsync(string relativeUri)
    {
        using var result = await HttpClient.GetAsync(BuildUri(relativeUri));
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsStringAsync();
        }
        else
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
            return default;
        }
    }

    public async Task<TReturn?> PostAsync<TReturn, TRequest>(string relativeUri, TRequest request)
    {
        using var result = await HttpClient.PostAsJsonAsync(BuildUri(relativeUri), request, options: s_jsonOptions);
        if (result.IsSuccessStatusCode)
        {
            return await ReadFromJson<TReturn>(result);
        }
        else
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
            return default;
        }
    }

    public async Task<string?> PostAsyncRaw<TRequest>(string relativeUri, TRequest request)
    {
        using var result = await HttpClient.PostAsJsonAsync(BuildUri(relativeUri), request, options: s_jsonOptions);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsStringAsync();
        }
        else
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
            return default;
        }
    }

    public async Task PostAsync(string relativeUri)
    {
        var result = await HttpClient.PostAsync(BuildUri(relativeUri), default);
        if (!result.IsSuccessStatusCode)
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
        }
    }

    public async Task<string?> PostAsync<TRequest>(string relativeUri, TRequest request)
    {
        using var result = await HttpClient.PostAsJsonAsync(BuildUri(relativeUri), request, options: s_jsonOptions);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsStringAsync();
        }
        else
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
            return default;
        }
    }

    public async Task DeleteAsync(string relativeUri)
    {
        using var result = await HttpClient.DeleteAsync(BuildUri(relativeUri));
        if (result.IsSuccessStatusCode) return;
        await FilterStatusCodes(result.StatusCode);
        var msg = await result.Content.ReadAsStringAsync();
        await _logger.LogError(msg);
    }

    public async Task PutAsync<TRequest>(string relativeUri, TRequest request)
    {
        using var result = await HttpClient.PutAsJsonAsync(BuildUri(relativeUri), request, options: s_jsonOptions);
        if (!result.IsSuccessStatusCode)
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
        }
    }

    public async Task PutAsync(string relativeUri)
    {
        using var result = await HttpClient.PutAsync(BuildUri(relativeUri), null);
        if (!result.IsSuccessStatusCode)
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
        }
    }

    public async Task<TReturn?> PutAsync<TReturn, TRequest>(string relativeUri, TRequest request)
    {
        using var result = await HttpClient.PutAsJsonAsync(BuildUri(relativeUri), request, options: s_jsonOptions);
        if (result.IsSuccessStatusCode)
        {
            return await ReadFromJson<TReturn>(result);
        }
        else
        {
            await FilterStatusCodes(result.StatusCode);
            var msg = await result.Content.ReadAsStringAsync();
            await _logger.LogError(msg);
            return default;
        }
    }

    private async Task<TReturn?> ReadFromJson<TReturn>(HttpResponseMessage result)
    {
        try
        {
            return await result.Content.ReadFromJsonAsync<TReturn>(s_jsonOptions);
        }
        catch (Exception e)
        {
            await _logger.LogError(e.Message);
            return default;
        }
    }
}
