using TransportApi.DTOs;

namespace TransportApi.Services;

public interface IStopTimeService
{
    Task<List<StopTimeDto>> GetStopScheduledStopTimes(string mode, string stopName, string timeString, bool before);
    Task<List<StopTimeDto>> GetTripScheduledStopTimes(string mode, string tripId, string timeString);
    Task<List<StopTimeDto>> GetTripRealtimeStopTimes(string mode, string tripId, string timeString);
}