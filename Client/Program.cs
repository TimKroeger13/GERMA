using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GERMAG.Client;
using GERMAG.Shared.Core;
using GERMAG.Client.Core;
using GERMAG.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<IErrorLogger, JsInterop>();
builder.Services.AddTransient<IGeothermalParameterService, GeothermalParameterService>();
builder.Services.AddTransient<IRestInteropFactory, RestInteropFactory>();
builder.Services.AddTransient<IUpdateDatabaseService, UpdateDatabaseService>();
builder.Services.AddTransient<ICalcualteResearchParameterService, CalcualteResearchParameterService>();

await builder.Build().RunAsync();
