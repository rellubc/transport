using Microsoft.AspNetCore.Mvc;

using TransportApi.DTOs;
using TransportApi.Services;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney")]
public class AgencyController(IAgencyService agencyService) : ControllerBase
{
    private readonly IAgencyService _agencyService = agencyService;

    [HttpGet("agencies")]
    public async Task<ActionResult<List<AgencyDto>>> GetAgencies()
    {
        var agencies = await _agencyService.GetAgencies();
        return Ok(agencies);
    }

    [HttpGet("agencies/{agencyId}")]
    public async Task<ActionResult<List<AgencyDto>>> GetAgency(string agencyId)
    {
        var agency = await _agencyService.GetAgency(agencyId);

        if (agency == null)
        {
            return NotFound();
        }

        return Ok(agency);
    }
}