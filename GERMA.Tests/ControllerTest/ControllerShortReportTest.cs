using Xunit;
using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.Controllers;
using GERMAG.Server.ReportCreation;
using Moq;
using GERMAG.DataModel.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GERMA.Tests;

public class ControllerShortReportTest
{
    private readonly ShortReport _shortReport;

    public ControllerShortReportTest()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=localhost:5433;Database=germa;Username=germa;Password=germa");
        var dataSource = dataSourceBuilder.ConfigureAndBuild();
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(dataSource, o => o.UseNetTopologySuite())
            .Options;
        var context = new DataContext(options);

        var findAllParameterForCoordinate = new FindAllParameterForCoordinate(context);
        var parameterDeserialator = new ParameterDeserialator();
        var createReportStructure = new CreateReportStructure();
        var geometryTransformation = new GeometryTransformation(context);
        var UserIdvalidation = new UserIdvalidation();
        
        var geometryFromGeoJson = new GeometryFromGeoJson(context, geometryTransformation, parameterDeserialator);

        var createReport = new CreateReport(findAllParameterForCoordinate, parameterDeserialator, createReportStructure);
        var receiveLandParcel = new ReceiveLandParcel(context, parameterDeserialator);
        var restrictionFromLandParcel = new RestrictionFromLandParcel(context, geometryFromGeoJson);
        var createFingerPrint = new CreateFingerPrint(context,UserIdvalidation);

        _shortReport = new ShortReport(createReport, receiveLandParcel, restrictionFromLandParcel, createFingerPrint);
    }

    [Fact]
    public async Task TestShortReportForFiveConnectedExampleLandParcels()
    {
        List<double> Xcor = [13.39911605567504, 13.399306441116495, 13.399443941713082, 13.399507403526927, 13.399465095651042];
        List<double> Ycor = [52.50729306308521, 52.507658618189446, 52.50712426574187, 52.50786463185578, 52.50807708243786];
        int Srid = 4326;

        //Act
        var result = await _shortReport.CalculateShortReport(Xcor, Ycor, Srid, "TestID");

        //Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestShortReportForFarSpreadLandparcelsThatAreNotConnected()
    {
        List<double> Xcor = [13.465083186976111,13.465235665544844];
        List<double> Ycor = [52.55052187178272,52.55007645875352];
        int Srid = 4326;

        //Act
        var result = await _shortReport.CalculateShortReport(Xcor, Ycor, Srid, "TestID");

        //result.First().Error

        //Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("Error",result.First().Error);
        
    }
}