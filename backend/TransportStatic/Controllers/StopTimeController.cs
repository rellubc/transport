using Microsoft.AspNetCore.Mvc;

using TransportStatic.DTOs;
using TransportStatic.Services;

namespace TransportStatic.Controllers;

[ApiController]
[Route("api/sydney")]
public class StopTimeController(IStopTimeService stopTimeService) : ControllerBase
{
    private readonly IStopTimeService _stopTimeService = stopTimeService;

    [HttpGet("stop/scheduled/stop-times")]
    public async Task<ActionResult<List<StopTimeDTO>>> GetSydneyStopScheduledStopTimes(string mode, string stopName, string timeString, bool before)
    {
        var stopTimes = await _stopTimeService.GetStopScheduledStopTimes(mode.ToLower(), stopName, timeString, before);
        return Ok(stopTimes); 
    }

    [HttpGet("trip/scheduled/stop-times")]
    public async Task<ActionResult<List<StopTimeDTO>>> GetSydneyTripScheduledStopTimes(string mode, string tripId, string timeString)
    {
        var stopTimes = await _stopTimeService.GetTripScheduledStopTimes(mode.ToLower(), tripId, timeString);
        return Ok(stopTimes); 
    }
}
