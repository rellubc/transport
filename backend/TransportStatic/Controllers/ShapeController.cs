using Microsoft.AspNetCore.Mvc;

using TransportStatic.DTOs;
using TransportStatic.Services;

namespace TransportStatic.Controllers;

[ApiController]
[Route("api/sydney")]
public class ShapeController(IShapeService shapeService) : ControllerBase
{
    private readonly IShapeService _shapeService = shapeService;

    [HttpGet("shapes")]
    public async Task<ActionResult<Dictionary<string, List<ShapeCoordinates>>>> GetSydneyShapes(string mode)
    {
        var shapes = await _shapeService.GetShapes(mode.ToLower());
        return Ok(shapes);
    }
}
