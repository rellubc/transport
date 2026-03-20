using TransportApi.DTOs;

namespace TransportApi.Services;

public interface IShapeService
{
    Task<Dictionary<string, List<ShapeDetails>>> GetShapes(string mode);
}