using TransportStatic.DTOs;

namespace TransportStatic.Services;

public interface IStopTimeService
{
    Task<List<StopTimeDTO>> GetStopScheduledStopTimes(string mode, string stopName, string timeString, bool before);
    Task<List<StopTimeDTO>> GetTripScheduledStopTimes(string mode, string tripId, string timeString);
}