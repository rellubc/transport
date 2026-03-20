using Microsoft.AspNetCore.Mvc;

using TransportApi.DTOs;
using TransportApi.Services;

namespace TransportApi.Controllers;

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
    public async Task<ActionResult<StopDto>> GetSydneyStops(string mode)
    {
        var stops = await _stopService.GetStops(mode);
        return Ok(stops);
    }
}
