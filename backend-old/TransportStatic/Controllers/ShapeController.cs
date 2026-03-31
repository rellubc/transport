using Microsoft.AspNetCore.Mvc;

using TransportStatic.DTOs;
using TransportStatic.Services;

namespace TransportStatic.Controllers;

[ApiController]
[Route("api/sydney")]
public class ShapeController(IShapeService stopService) : ControllerBase
{
    private readonly IShapeService _stopService = stopService;

    [HttpGet("shapes")]
    public async Task<ActionResult<Dictionary<string, List<ShapeDetails>>>> GetSydneyShapes(string mode)
    {
        var shapes = await _stopService.GetShapes(mode);
        return Ok(shapes);
    }
}
