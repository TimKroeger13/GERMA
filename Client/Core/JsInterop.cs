using GERMAG.Shared.Core;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace GERMAG.Client.Core;

public class JsInterop(IJSRuntime jsRuntime) : IErrorLogger
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    [DoesNotReturn]
    public async Task LogError(string error)
    {
        await _jsRuntime.InvokeVoidAsync("alert", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") + "\n" + error);
        throw new Exception(error);
    }

    public async Task Prompt(string message, string title)
    {
        await _jsRuntime.InvokeVoidAsync("alert", title + "\n" + message);
    }

    public async ValueTask<bool> Confirm(string message, string title)
    {
        return await _jsRuntime.InvokeAsync<bool>("confirm", title + "\n" + message);
    }
}
