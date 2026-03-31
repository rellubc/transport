using TransportStatic.DTOs;

namespace TransportStatic.Services;

public interface ITripService
{
    Task<List<TripDTO>> GetTrips();
    Task<TripDTO?> GetTrip(string tripId);
}