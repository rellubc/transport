using TransitRealtime;
using TransportStatic.DTOs.Realtime;

namespace TransportStatic.Services;

public interface IRealtimeService
{
    Task<TripUpdateDTO> GetRealtimeTripUpdate(string mode, string tripId);
    Task<List<VehiclePositionDTO>> GetRealtimeVehicles(string mode);
    Task<List<TripUpdate>> GetRealtimeTripUpdates(string mode);
    Task<List<VehiclePosition>> GetRealtimeVehiclePositions(string mode);
}