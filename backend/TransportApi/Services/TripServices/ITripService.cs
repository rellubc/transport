using TransportApi.DTOs;
using TransportApi.DTOs.Realtime;

namespace TransportApi.Services;

public interface ITripService
{
    Task<List<TripDto>> GetTrips();
    Task<TripDto?> GetTrip(string tripId);
}