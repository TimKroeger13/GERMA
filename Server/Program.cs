using GERMAG.DataModel.Database;
using GERMAG.Server;
using GERMAG.Server.Core.Configurations;
using GERMAG.Server.DataPulling;
using GERMAG.Server.DataPulling.JsonDeserialize;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.ReportCreation;
using GERMAG.Server.Research;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;
using System.Media;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var options = builder.Configuration.GetSection(ConfigurationOptions.Configuration).Get<ConfigurationOptions>()
    ?? throw new Exception("Configuration could not be found");
IEnviromentConfiguration configuration = builder.Environment.IsDevelopment() ?
    new DebugConfiguration(options) : new ReleaseConfiguration(options);
builder.Services.AddSingleton(configuration);
builder.Services.AddTransient<HttpClient>();
builder.Services.AddHttpClient(HttpClients.LongTimeoutClient, o => o.Timeout = TimeSpan.FromMinutes(120));
builder.Services.AddHttpClient(HttpClients.Default);
builder.Services.AddCors(options => options.AddPolicy(CorsPolicies.GetAllowed, policy => policy.WithMethods("GET").AllowAnyHeader().AllowAnyOrigin()));
builder.Services.AddTransient<IDataFetcher, DataFetcher>();
builder.Services.AddTransient<IDatabaseUpdater, DatabaseUpdater>();
builder.Services.AddTransient<ICalcualteAllParameterForArea, CalcualteAllParameterForArea>();
builder.Services.AddTransient<IJsonDeserialize, JsonDeserialize>();
builder.Services.AddTransient<ICreateReportAsync, CreateReport>();
builder.Services.AddTransient<IParameterDeserialator, ParameterDeserialator>();
builder.Services.AddTransient<IFindAllParameterForCoordinate, FindAllParameterForCoordinate>();
builder.Services.AddTransient<ICreateReportStructure, CreateReportStructure>();
builder.Services.AddTransient<IReceiveLandParcel, ReceiveLandParcel>();
builder.Services.AddTransient<IRestrictionFromLandParcel, RestrictionFromLandParcel>();
builder.Services.AddTransient<IGeoThermalProbesCalcualtion, GeoThermalProbesCalcualtion>();
builder.Services.AddTransient<IGetProbeSpecificData, GetProbeSpecificData>();
builder.Services.AddTransient<IGetPolylineData, GetPolylineData>();
builder.Services.AddTransient<IGetProbeSepcificDataSingleProbe, GetProbeSepcificDataSingleProbe>();
builder.Services.AddTransient<IFindLocalDirectoryPath, FindLocalDirectoryPath>();
builder.Services.AddTransient<IRating, Rating>();
builder.Services.AddTransient<ICrossInfluence, CrossInfluence>();
builder.Services.AddTransient<IGeometryFromGeoJson, GeometryFromGeoJson>();
builder.Services.AddTransient<IGeometryTransformation, GeometryTransformation>();
builder.Services.AddTransient<IFilterBheSubGroups, FilterBheSubGroups>();
builder.Services.AddTransient<IReciveBoundingBoxInformation, ReciveBoundingBoxInformation>();
builder.Services.AddTransient<IEnergyDemand, EnergyDemand>();
builder.Services.AddTransient<IThermalConductivity, ThermalConductivity>();
builder.Services.AddTransient<IExtractionCalculation, ExtractionCalculation>();
builder.Services.AddTransient<IGetTemplateBHE, GetTemplateBHE>();
builder.Services.AddTransient<IGetTemplateBasedProbeSpecificData, GetTemplateBasedProbeSpecificData>();
builder.Services.AddTransient<IFilterAreaForBHECount, FilterAreaForBHECount>();
builder.Services.AddTransient<IUserIdvalidation, UserIdvalidation>();
builder.Services.AddTransient<ICreateFingerPrint, CreateFingerPrint>();
builder.Services.AddTransient<IShortReport, ShortReport>();
builder.Services.AddTransient<ICoreReport, CoreReport>();
builder.Services.AddTransient<IFullreport, Fullreport>();
builder.Services.AddTransient<ICustomReport, CustomReport>();
builder.Services.AddTransient<IUpdateVersion, UpdateVersion>();
var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.DatabaseConnection);
var dataSource = dataSourceBuilder.ConfigureAndBuild();

//Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Short Report
    options.AddFixedWindowLimiter("fixedShortReport", opt =>
    {
        opt.PermitLimit = 15;
        opt.Window = TimeSpan.FromSeconds(10);
    });

    // Full Report
    options.AddFixedWindowLimiter("fixedFullReport", opt =>
    {
        opt.PermitLimit = 30; 
        opt.Window = TimeSpan.FromSeconds(10);
    });

    options.OnRejected = async (context, token) =>
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var rateLimitPolicy = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
        
        int statusCode = 429; 
        string message = "Rate limit exceeded";
        
        // Cusom error codes
        if (rateLimitPolicy?.PolicyName == "fixedShortReport")
        {
            statusCode = 516;
            message = "Short report: You can only make 15 requests every 10 seconds. Please wait before trying again.";
        }
        else if (rateLimitPolicy?.PolicyName == "fixedFullReport")
        {
            statusCode = 517;
            message = "Full report: You can only make 4 requests every 10 seconds. Please wait before trying again.";
        }

        context.HttpContext.Response.StatusCode = statusCode;
        var response = new
        {
            error = "Rate limit exceeded",
            message = message,
            retryAfter = 10
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: token);
    };
});

builder.Services.AddControllers();


builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(dataSource, npg =>
    {
        npg.UseNetTopologySuite();
        npg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(CorsPolicies.GetAllowed);

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");



Console.WriteLine(@"
 ██████╗ ███████╗██████╗ ███╗   ███╗ █████╗ 
██╔════╝ ██╔════╝██╔══██╗████╗ ████║██╔══██╗
██║  ███╗█████╗  ██████╔╝██╔████╔██║███████║
██║   ██║██╔══╝  ██╔══██╗██║╚██╔╝██║██╔══██║
╚██████╔╝███████╗██║  ██║██║ ╚═╝ ██║██║  ██║
 ╚═════╝ ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝  ╚═╝");

Console.WriteLine(@"   ___         _   _                       _   ___                          ___                              
  / __|___ ___| |_| |_  ___ _ _ _ __  __ _| | | __|_ _  ___ _ _ __ _ _  _  | _ \___ ___ ___ _  _ _ _ __ ___  
 | (_ / -_) _ \  _| ' \/ -_) '_| '  \/ _` | | | _|| ' \/ -_) '_/ _` | || | |   / -_|_-</ _ \ || | '_/ _/ -_) 
  \___\___\___/\__|_||_\___|_| |_|_|_\__,_|_| |___|_||_\___|_| \__, |\_, | |_|_\___/__/\___/\_,_|_| \__\___| 
  __  __                _                          _     _     |___/ |__/        _                           
 |  \/  |__ _ _ __ _ __(_)_ _  __ _   __ _ _ _  __| |   /_\  _ _  __ _| |_  _ __(_)___                       
 | |\/| / _` | '_ \ '_ \ | ' \/ _` | / _` | ' \/ _` |  / _ \| ' \/ _` | | || (_-< (_-<                       
 |_|  |_\__,_| .__/ .__/_|_||_\__, | \__,_|_||_\__,_| /_/ \_\_||_\__,_|_|\_, /__/_/__/                       
             |_|  |_|         |___/                                      |__/                                
             ");

Console.WriteLine("Application started successfully!");
Console.WriteLine("");
Console.WriteLine($"Listening on: {string.Join(", ", builder.WebHost.GetSetting("urls") ?? "ASPNETCORE_URLS not found")}");
Console.WriteLine("");


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

    var connectionString = dbContext.Database.GetConnectionString();
    Console.WriteLine("Connecting to database:");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine($"{connectionString}");
    Console.ResetColor();

    try
    {
        if (dbContext.Database.CanConnect())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Database connection established successfully!!! WOOO!!!");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database connection failed!");
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Database connection error: \n{ex.Message}");
        Console.ResetColor();
    }
}
Console.WriteLine("");

Console.WriteLine("\a");

app.UseRateLimiter(); 
app.MapControllers();

app.Run();