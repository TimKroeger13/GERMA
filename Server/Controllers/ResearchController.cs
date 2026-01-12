using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.DataModel;
using GERMAG.Server.DataPulling;
using GERMAG.Server.Research;
using Microsoft.AspNetCore.Cors;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchController(ICalcualteAllParameterForArea calcualteAllParameterForArea) : Controller
{
    [HttpGet("research")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<string> calcualteResearch()
    {
        return await calcualteAllParameterForArea.CalucalteAllParameters();
    }
}
