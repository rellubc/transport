using Microsoft.AspNetCore.Mvc;

using TransportApi.DTOs.Realtime;
using TransportApi.Services;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney")]
public class RealtimeController(IRealtimeService realtimeService) : ControllerBase
{
    private readonly IRealtimeService _realtimeService = realtimeService;

    [HttpGet("realtime/trip-updates")]
    public async Task<ActionResult<TripUpdateDto>> GetRealtimeUpdates(string mode, string tripId)
    {
        var tripUpdate = await _realtimeService.GetRealtimeTripUpdate(mode, tripId);
        return Ok(tripUpdate);
    }

    [HttpGet("realtime/vehicles")]
    public async Task<ActionResult<List<VehiclePositionDto>>> GetRealtimeVehicles(string mode)
    {
        var vehiclePositions = await _realtimeService.GetRealtimeVehicles(mode);
        return Ok(vehiclePositions);
    }
}