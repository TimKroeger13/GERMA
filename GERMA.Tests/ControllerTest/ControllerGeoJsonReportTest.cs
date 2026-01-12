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

namespace GERMA.Tests;

public class ControllerGeoJsonReportTest
{

    private readonly CustomReport _customReport;

    public ControllerGeoJsonReportTest()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=localhost:5433;Database=germa;Username=germa;Password=germa");
        var dataSource = dataSourceBuilder.ConfigureAndBuild();
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(dataSource, o => o.UseNetTopologySuite())
            .Options;
        var context = new DataContext(options);

        var geometryTransformation = new GeometryTransformation(context);
        var UserIdvalidation = new UserIdvalidation();
        var findAllParameterForCoordinate = new FindAllParameterForCoordinate(context);
        var parameterDeserialator = new ParameterDeserialator();
        var createReportStructure = new CreateReportStructure();
        var filterAreaForBHECount = new FilterAreaForBHECount();
        var crossInfluence = new CrossInfluence();
        var filterBheSubGroups = new FilterBheSubGroups();
        var energyDemand = new EnergyDemand(geometryTransformation);
        var thermalConductivity = new ThermalConductivity();
        var extractionCaclculation = new ExtractionCalculation();
        var getTemplateBasedProbeSpecificData = new GetTemplateBasedProbeSpecificData(extractionCaclculation);

        var getTemplateBHE = new GetTemplateBHE(context, parameterDeserialator, thermalConductivity, extractionCaclculation);
        var createReport = new CreateReport(findAllParameterForCoordinate, parameterDeserialator, createReportStructure);
        var geometryFromGeoJson = new GeometryFromGeoJson(context, geometryTransformation, parameterDeserialator);
        var RestrictionFromLandParcel = new RestrictionFromLandParcel(context, geometryFromGeoJson);
        var GeoThermalProbesCalcualtion = new GeoThermalProbesCalcualtion(filterAreaForBHECount);
        var createFingerPrint = new CreateFingerPrint(context, UserIdvalidation);
        var coreReport = new CoreReport(createReport, RestrictionFromLandParcel, GeoThermalProbesCalcualtion, crossInfluence, filterBheSubGroups,
            energyDemand, getTemplateBHE, getTemplateBasedProbeSpecificData);

        _customReport = new CustomReport(geometryFromGeoJson, createFingerPrint, coreReport);
    }

    [Fact] //Do not find LanpParcel 80
    public async Task TestGeoJsonReportForSelectedLandparcelsExtendedByACustomGeometryThatIntersetcsTwoDifferentLandParcels()
    {
        //Arrage

        GeoJsonRequest request = new()
        {
            Srid = 4326,
            Geojson = "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[13.398903006958614,52.53813304578084],[13.39906503374492,52.537919080757874],[13.399217129954813,52.53796944917711],[13.399229111989081,52.53795366400251],[13.399238650527154,52.53794110019663],[13.399103408510127,52.53790611529359],[13.399252643708007,52.53769474269975],[13.399390898323881,52.537740527548955],[13.399404199175645,52.537723002156184],[13.399410738418604,52.53772483485722],[13.399437537660807,52.53773234517367],[13.399490668803502,52.53774724258451],[13.399559301482423,52.53776648547942],[13.399807092378493,52.537835947234704],[13.399630707253225,52.53806666404117],[13.399569801485724,52.53814631927499],[13.399563669995139,52.53815435527266],[13.39955961409797,52.53815965009645],[13.39945408708917,52.53829771007663],[13.399213969970127,52.53823005070018],[13.39920252666984,52.53822682997349],[13.399191039810393,52.5382235906744],[13.399053604742463,52.53818487735417],[13.3990632692824,52.538172141902805],[13.399073292258082,52.53815893992464],[13.398903006958614,52.53813304578084]]]}}",
            GeojsonMinus = "null",
            ProbeDistance = 6,
            UseBuildings = true,
            LandParcelDistance = 2,
            UseTrees = true,
            TreeBuffer = 0,
            MinimalBhePerInduvidualArea = 0,
            EnergyDemand = 0,
            FlowIsTurbolent = false,
            Regeneration = "None",
            CalculateMaximalFieldSize = false,
            MaximalDrillingDepth = 400,
            UserId = "TestID",
            Cop = 3
        };

        //Act

        var result = await _customReport.CalcualteCustomReport(request);

        //Assert
        Assert.NotNull(result.First().Land_parcel_number);
        Assert.DoesNotContain("80", result.First().Land_parcel_number);
    }

    [Fact]
    public async Task TestGeoJsonReportForFarSpreadLandparcelsThatAreNotConnectedAndANegativeArea()
    {
        GeoJsonRequest request = new()
        {
            CalculateMaximalFieldSize = false,
            Cop = 4,
            EnergyDemand = 367990,
            FlowIsTurbolent = false,
            GeojsonSuperior = "null",
            LandParcelDistance = 2,
            MaximalDrillingDepth = 400,
            MinimalBhePerInduvidualArea = 0,
            ProbeDistance = 6,
            Regeneration = "None",
            TreeBuffer = 0,
            UseBuildings = true,
            UseTrees = true,
            Geojson = "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[13.399557001402837,52.489591050239355],[13.399621492376962,52.48967403856046],[13.399681340327156,52.48975103888985],[13.399997974380186,52.49015847437052],[13.399526019629565,52.490254076253514],[13.398133455247756,52.49053614215425],[13.398021674300962,52.490558796578746],[13.397999412355762,52.49047951716794],[13.397818985794041,52.489836546709846],[13.397767267481974,52.48965227465192],[13.398779058266147,52.48954739051104],[13.398877008135798,52.48953723464283],[13.399398046390766,52.489386536626576],[13.399557001402837,52.489591050239355]]],[[[13.401580620881235,52.49219704270691],[13.401552301804724,52.49220741342796],[13.401419684936196,52.49223345790803],[13.399927494008814,52.492526480681256],[13.399699636534505,52.49211418621634],[13.399641294081647,52.492007400782704],[13.39962685492448,52.49201034334401],[13.39944275478577,52.491737876601434],[13.399273323690078,52.491618025339925],[13.398471489712097,52.491770784010875],[13.398443579832572,52.491786095650276],[13.398368446998623,52.49179610155794],[13.398172149272115,52.49109549071791],[13.400380129871998,52.49064996271826],[13.401146702680258,52.49163787516812],[13.401580620881235,52.49219704270691]]]]}}",
            GeojsonMinus = "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[13.40675131606482,52.510568435324856],[13.40690013889448,52.51066240128821],[13.407013134005906,52.51051306243099],[13.40678714378305,52.51043251991535],[13.40675131606482,52.510568435324856]]]}}]}",
            Srid = 4326,
            UserId = "TestID",
        };

        //Act

        var result = await _customReport.CalcualteCustomReport(request);

        //Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("Error",result.First().Error);
        
    }

    [Fact]
    public async Task TestGeoJsonReportForAnMultipolyonThatOnlyContainsOneElementLikeAPolygon()
    {
        GeoJsonRequest request = new()
        {
            CalculateMaximalFieldSize = false,
            Cop = 4,
            EnergyDemand = 59460,
            FlowIsTurbolent = false,
            GeojsonSuperior = "null",
            LandParcelDistance = 2,
            MaximalDrillingDepth = 400,
            MinimalBhePerInduvidualArea = 0,
            ProbeDistance = 6,
            Regeneration = "None",
            TreeBuffer = 0,
            UseBuildings = true,
            UseTrees = true,
            Geojson = "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[13.519415627795544,52.49452852608103],[13.518498977256627,52.49427804362393],[13.518814713735324,52.493828285424385],[13.51884016490696,52.49383451114343],[13.519073980358488,52.493891518967395],[13.519077837664467,52.49389245735918],[13.51960841243273,52.4940218605923],[13.519760259205784,52.49430417113879],[13.519684310197789,52.49441463700315],[13.51960143346336,52.49452653560485],[13.519568935466053,52.49457045444886],[13.519415627795544,52.49452852608103]]]}}",
            GeojsonMinus = "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[13.518773317337038,52.49432819565086],[13.519089818000795,52.49421705871253],[13.519036173820497,52.4939719027083],[13.518773317337038,52.49432819565086]]]}}]}",
            Srid = 4326,
            UserId = "TestID",
        };

        //Act

        var result = await _customReport.CalcualteCustomReport(request);

        //Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("Error",result.First().Error);
        
    }
}