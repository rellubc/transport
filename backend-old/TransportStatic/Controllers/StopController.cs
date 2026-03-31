using Microsoft.AspNetCore.Mvc;

using TransportStatic.DTOs;
using TransportStatic.Services;

namespace TransportStatic.Controllers;

[ApiController]
[Route("api/sydney")]
public class StopController : ControllerBase
{
    private readonly IStopService _stopService;

    public StopController(IStopService stopService)
    {
        _stopService = stopService;
    }

    [HttpGet("stops")]
    public async Task<ActionResult<StopDTO>> GetSydneyStops(string mode)
    {
        var stops = await _stopService.GetStops(mode);
        return Ok(stops);
    }
}
