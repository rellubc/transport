using TransitRealtime;
using TransportApi.DTOs.Realtime;

namespace TransportApi.Services;

public interface IRealtimeService
{
    Task<TripUpdateDto> GetRealtimeTripUpdate(string mode, string tripId);
    Task<List<VehiclePositionDto>> GetRealtimeVehicles(string mode);
    Task<List<TripUpdate>> GetRealtimeTripUpdates(string mode);
    Task<List<VehiclePosition>> GetRealtimeVehiclePositions(string mode);
}