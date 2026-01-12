using Microsoft.AspNetCore.Mvc;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.DataModel;
using GERMAG.Server.DataPulling;
using GERMAG.Server.Research;
using Microsoft.AspNetCore.Cors;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConnectionController(ICreateFingerPrint createFingerPrint) : Controller
{
    [HttpGet("connectionstart")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task TrackInitalConnection(string userid)
    {
        await createFingerPrint.CreateUniqueFingerPrint(userid, FingerPrintTypes.sessionLoaded, null);
    }

    [HttpPost("connectionend")]
    [EnableCors(CorsPolicies.GetAllowed)]
    public async Task<IActionResult> TrackEndConnection([FromQuery] string userid)
    {
        await createFingerPrint.CreateUniqueFingerPrint(userid, FingerPrintTypes.sessionEnded, null);

        return Ok();
    }
}

