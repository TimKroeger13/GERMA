using Xunit;
using GERMAG.Server.GeometryCalculations;
using Moq;
using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using Microsoft.EntityFrameworkCore;

namespace GERMA.Tests;

public class GetLandParcelTest()
{
    [Fact]

    public async Task TestGetLandParcelForFiveConnectedExampleLandParcels()
    {
        //Arrage

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql("Host=localhost:5433;Database=germa;Username=germa;Password=germa",
                o => o.UseNetTopologySuite())
            .Options;
        var context = new DataContext(options);

        var parameterDeserialator = new ParameterDeserialator();

        //var _GetLandParcel = new ReceiveLandParcel(contextMock.Object, parameterDeserialatorMock.Object);
        var getLandParcel = new ReceiveLandParcel(context,parameterDeserialator);


        List<double> Xcor = [13.39911605567504, 13.399306441116495, 13.399443941713082, 13.399507403526927, 13.399465095651042];
        List<double> Ycor = [52.50729306308521, 52.507658618189446, 52.50712426574187, 52.50786463185578, 52.50807708243786];
        int Srid = 4326;

        //Act
        var result = await getLandParcel.GetLandParcel(Xcor, Ycor, Srid);


        //Assert
        Assert.NotNull(result);
        
    }
}