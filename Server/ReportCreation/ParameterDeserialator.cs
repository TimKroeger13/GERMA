using GERMAG.DataModel.Database;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using System.Text.Json;
using Properties = GERMAG.Shared.Properties;

namespace GERMAG.Server.ReportCreation;

public interface IParameterDeserialator
{
    Properties DeserializeParameters(string SerializedInputJson);
}

public class ParameterDeserialator : IParameterDeserialator
{
    public Properties DeserializeParameters(string SerializedInputJson)
    {
        return JsonSerializer.Deserialize<Properties>(SerializedInputJson, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new Exception("No wfs found (ParameterRoot)");
    }
}
