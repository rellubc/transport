using TransportStatic.DTOs;

namespace TransportStatic.Services;

public interface IShapeService
{
    Task<Dictionary<string, List<ShapeDetails>>> GetShapes(string mode);
}