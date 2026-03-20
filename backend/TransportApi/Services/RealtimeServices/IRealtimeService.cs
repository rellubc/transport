using TransportApi.DTOs.Realtime;

namespace TransportApi.Services;

public interface IRealtimeService
{
    Task<TripUpdateDto> GetRealtimeTripUpdate(string TripId, string mode);
    Task<List<VehiclePositionDto>> GetRealtimeVehicles(string mode);
}