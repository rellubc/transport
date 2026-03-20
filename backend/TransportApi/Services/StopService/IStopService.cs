using TransportApi.DTOs;

namespace TransportApi.Services;

public interface IStopService
{
    Task<List<StopDto>> GetStops(string mode);
}