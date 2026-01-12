using GERMAG.Client.Core;
using GERMAG.DataModel;
using GERMAG.Shared.Core;
using System.Security.AccessControl;

namespace GERMAG.Client.Services;

public interface IGeothermalParameterService
{
    Task AddParameter(GeothermalParameterModel model);

    Task DeleteParameter(long parameterId);

    Task<IEnumerable<GeothermalParameterModel>?> GetParameters();

    Task UpdateParameter(GeothermalParameterModel model);
}

public class GeothermalParameterService(IRestInteropFactory factory) : IGeothermalParameterService
{
    private readonly IRestInterop _restInterop = factory.CreateRestInterop("api/GeothermalParameter");

    public Task<IEnumerable<GeothermalParameterModel>?> GetParameters()
    {
        return _restInterop.GetAsync<IEnumerable<GeothermalParameterModel>>("parameter");
    }

    public Task DeleteParameter(long parameterId)
    {
        return _restInterop.DeleteAsync("deleteparameter?id=" + parameterId);
    }

    public Task UpdateParameter(GeothermalParameterModel model)
    {
        return _restInterop.PutAsync("updateparameter", model);
    }

    public Task AddParameter(GeothermalParameterModel model)
    {
        return _restInterop.PostAsync("addparameter", model);
    }
}
