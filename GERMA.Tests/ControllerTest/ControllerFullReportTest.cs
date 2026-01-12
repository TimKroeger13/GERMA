using Xunit;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.Controllers;
using GERMAG.Server.ReportCreation;
using Moq;
using GERMAG.DataModel.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using GERMAG.Shared;
using Npgsql.Replication;
using System.Net.Http.Headers;

namespace GERMA.Tests;

public class ControllerFullReportTest
{
    private readonly Fullreport _fullReport;
    public ControllerFullReportTest()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=localhost:5433;Database=germa;Username=germa;Password=germa");
        var dataSource = dataSourceBuilder.ConfigureAndBuild();
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(dataSource, o => o.UseNetTopologySuite())
            .Options;
        var context = new DataContext(options);

        var geometryTransformation= new GeometryTransformation(context);
        var parameterDeserialator = new ParameterDeserialator();

        var userIdvalidation = new UserIdvalidation();
        var findAllParameterForCoordinate = new FindAllParameterForCoordinate(context);
        var createReportStructure = new CreateReportStructure();
        var geometryFromGeoJson = new GeometryFromGeoJson(context,geometryTransformation,parameterDeserialator);
        var filterAreaForBHECount = new FilterAreaForBHECount();
        var thermalConductivity = new ThermalConductivity();
        var extractionCaclculation = new ExtractionCalculation();

        var receiveLandParcel = new ReceiveLandParcel(context,parameterDeserialator);
        var createFingerPrint = new CreateFingerPrint(context,userIdvalidation);
        var CreateReport = new CreateReport(findAllParameterForCoordinate,parameterDeserialator,createReportStructure);
        var restrictionFromLandParcel = new RestrictionFromLandParcel(context,geometryFromGeoJson);
        var geoThermalProbesCalcualtion = new GeoThermalProbesCalcualtion(filterAreaForBHECount);
        var crossInfluence = new CrossInfluence();
        var filterBheSubGroups = new FilterBheSubGroups();
        var energyDemand = new EnergyDemand(geometryTransformation);
        var getTemplateBHE = new GetTemplateBHE(context,parameterDeserialator,thermalConductivity,extractionCaclculation);
        var getTemplateBasedProbeSpecificData = new GetTemplateBasedProbeSpecificData(extractionCaclculation);

        var coreReport = new CoreReport(CreateReport,restrictionFromLandParcel,geoThermalProbesCalcualtion,crossInfluence,filterBheSubGroups,energyDemand,getTemplateBHE,getTemplateBasedProbeSpecificData);

        _fullReport = new Fullreport(receiveLandParcel,createFingerPrint,coreReport);
    }

    [Fact]
    public async Task TestFullReportForASingleLandparcel_normalExpectedCase()
    {
        //Arrage

        List<double> Xcor = [13.465055945991722];
        List<double> Ycor = [52.55044169030573];
        int Srid = 4326;
        double ProbeDistance = 6;
        bool UseBuildings = true;
        bool UseTrees = true;
        double LandParcelDistance = 2;
        double TreeBuffer = 0;
        int MinimalBhePerInduvidualArea = 0;
        int EnergyDemand = 0;
        bool FlowIsTurbolent = false;
        Regeneration Regeneration = Regeneration.None;
        bool CalculateMaximalFieldSize = false;
        int MaximalDrillingDepth = 400;
        string? userId = "TestID";
        double? cop = 4;

        //Act
        var result = await _fullReport.CalcualteFullReport(Xcor,Ycor,Srid,ProbeDistance,UseBuildings,UseTrees,LandParcelDistance,TreeBuffer,MinimalBhePerInduvidualArea,EnergyDemand,FlowIsTurbolent,
        Regeneration,CalculateMaximalFieldSize,MaximalDrillingDepth,userId,cop);

        //Assert
        Assert.NotNull(result);
        
    }
}