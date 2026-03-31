using Microsoft.AspNetCore.Mvc;

using TransportStatic.DTOs;
using TransportStatic.Services;

namespace TransportStatic.Controllers;

[ApiController]
[Route("api/sydney")]
public class AgencyController(IAgencyService agencyService) : ControllerBase
{
    private readonly IAgencyService _agencyService = agencyService;

    [HttpGet("agencies")]
    public async Task<ActionResult<List<AgencyDTO>>> GetAgencies()
    {
        var agencies = await _agencyService.GetAgencies();
        return Ok(agencies);
    }

    [HttpGet("agencies/{agencyId}")]
    public async Task<ActionResult<List<AgencyDTO>>> GetAgency(string agencyId)
    {
        var agency = await _agencyService.GetAgency(agencyId);

        if (agency == null)
        {
            return NotFound();
        }

        return Ok(agency);
    }
}