using Microsoft.AspNetCore.Mvc;

using TransportApi.DTOs;
using TransportApi.Services;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney")]
public class TripController(ITripService tripService) : ControllerBase
{
    private readonly ITripService _tripService = tripService;

    [HttpGet("trips")]
    public async Task<ActionResult<TripDto>> GetSydneyTrips()
    {
        var trips = await _tripService.GetTrips();
        return Ok(trips);
    }

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<TripDto>> GetSydneyTrips(string tripId)
    {
        var trip = await _tripService.GetTrip(tripId);

        if (trip == null)
        {
            return NotFound();
        }

        return Ok(trip);
    }
}
