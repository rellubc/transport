using TransportStatic.DTOs;

namespace TransportStatic.Services;

public interface IShapeService
{
    Task<Dictionary<string, List<ShapeCoordinates>>> GetShapes(string mode);
}