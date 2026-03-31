using TransportStatic.DTOs;

namespace TransportStatic.Services;

public interface IStopService
{
    Task<List<StopDTO>> GetStops(string mode);
}