using Microsoft.AspNetCore.Mvc;

using TransportStatic.DTOs.Realtime;
using TransportStatic.Services;

namespace TransportStatic.Controllers;

[ApiController]
[Route("api/sydney")]
public class RealtimeController(IRealtimeService realtimeService) : ControllerBase
{
    private readonly IRealtimeService _realtimeService = realtimeService;

    [HttpGet("realtime/trip-updates")]
    public async Task<ActionResult<TripUpdateDTO>> GetRealtimeUpdates(string mode, string tripId)
    {
        var tripUpdate = await _realtimeService.GetRealtimeTripUpdate(mode, tripId);
        return Ok(tripUpdate);
    }

    [HttpGet("realtime/vehicles")]
    public async Task<ActionResult<List<VehiclePositionDTO>>> GetRealtimeVehicles(string mode)
    {
        var vehiclePositions = await _realtimeService.GetRealtimeVehicles(mode);
        return Ok(vehiclePositions);
    }
}