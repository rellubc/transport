using Microsoft.AspNetCore.Mvc;

using TransportApi.DTOs;
using TransportApi.Services;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney")]
public class StopTimeController(IStopTimeService stopTimeService) : ControllerBase
{
    private readonly IStopTimeService _stopTimeService = stopTimeService;

    [HttpGet("stop/scheduled/stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyStopScheduledStopTimes(string mode, string stopName, string timeString, bool before)
    {
        var stopTimes = await _stopTimeService.GetStopScheduledStopTimes(mode, stopName, timeString, before);
        return Ok(stopTimes); 
    }

    [HttpGet("trip/scheduled/stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyTripScheduledStopTimes(string mode, string tripId, string timeString)
    {
        var stopTimes = await _stopTimeService.GetTripScheduledStopTimes(mode, tripId, timeString);
        return Ok(stopTimes); 
    }

    [HttpGet("trip/realtime/stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyTripRealtimeStopTimes(string mode, string tripId, string timeString)
    {
        var stopTimes = await _stopTimeService.GetTripRealtimeStopTimes(mode, tripId, timeString);
        return Ok(stopTimes); 
    }
}
