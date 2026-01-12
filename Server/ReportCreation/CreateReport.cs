using GERMAG.DataModel.Database;
using GERMAG.Server.DataPulling.JsonDeserialize;
using GERMAG.Shared;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace GERMAG.Server.ReportCreation;

public interface ICreateReportAsync
{
    Task<IEnumerable<Report>> CreateGeothermalReportAsync(LandParcel landParcelElement, Restricion RestrictionFile, bool useLandParcel);
}

public class CreateReport(IFindAllParameterForCoordinate findAllParameterForCoordinate, IParameterDeserialator parameterDeserialator, ICreateReportStructure createReportStructure) : ICreateReportAsync
{
    public async Task<IEnumerable<Report>> CreateGeothermalReportAsync(LandParcel landParcelElement, Restricion RestrictionFile, bool useLandParcel)
    {
        var ParameterList = await Task.Run(() => findAllParameterForCoordinate.FindCoordianteParameters(landParcelElement, RestrictionFile, useLandParcel));

        var jsonData_Parameter = await Task.Run(() => ParameterList.ConvertAll(p => parameterDeserialator.DeserializeParameters(p.Parameter ?? "")));

        var mergedList = ParameterList.Zip(jsonData_Parameter, (original, jsonData) =>
        new GeometryElementParameter
        {
            Type = original.Type,
            ParameterKey = original.ParameterKey,
            Parameter = original.Parameter,
            JsonDataParameter = jsonData
        })
        .ToList();

        var CompleteReport = await Task.Run(() => createReportStructure.CreateReport(mergedList));

        return CompleteReport;
    }
}
