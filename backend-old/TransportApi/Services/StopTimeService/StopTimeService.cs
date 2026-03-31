using Microsoft.EntityFrameworkCore;

using TransportStatic.Data;
using TransportStatic.DTOs;

namespace TransportStatic.Services;

public class StopTimeService(TransportDbContext db, IRealtimeService realtimeService) : IStopTimeService
{
    private readonly TransportDbContext _db = db;
    private readonly IRealtimeService _realtimeService = realtimeService;

    public async Task<List<StopTimeDTO>> GetStopScheduledStopTimes(string mode, string stopName, string timeString, bool before)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));

        var stopTimesQuery = await QueryStopStopTimes(mode, stopName, time);
        var stopTimes = await SliceStopTimes(stopTimesQuery, time, before);

        return stopTimes;
    }

    public async Task<List<StopTimeDTO>> GetTripScheduledStopTimes(string mode, string tripId, string timeString)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));

        var stopTimesQuery = await QueryTripStopTimes(mode, tripId, time);
        var stopTimes = await SortStopTimes(stopTimesQuery, time);

        return stopTimes;
    }

    public async Task<List<StopTimeDTO>> GetTripRealtimeStopTimes(string mode, string tripId, string timeString)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));

        var stopTimesQuery = await QueryTripStopTimes(mode, tripId, time);
        var stopTimes = await SortStopTimes(stopTimesQuery, time);

        var tripUpdate = await _realtimeService.GetRealtimeTripUpdate(mode, tripId);

        for (var i = 0; i < stopTimes.Count; i++)
        {
            var corresp = tripUpdate.StopTimeUpdate.Find((stu) => stu.StopId == stopTimes[i].StopId);
            if (corresp == null) continue;

            if (corresp.Arrival != null && corresp.Arrival.Delay != null)
            {
                var scheduledArrivalTime = DateTime.Today;
                scheduledArrivalTime = scheduledArrivalTime.AddHours(stopTimes[i].ArrivalTime.Hours);
                scheduledArrivalTime = scheduledArrivalTime.AddMinutes(stopTimes[i].ArrivalTime.Minutes);
                scheduledArrivalTime = scheduledArrivalTime.AddSeconds(stopTimes[i].ArrivalTime.Seconds);
                scheduledArrivalTime = scheduledArrivalTime.AddSeconds((double)corresp.Arrival.Delay);
                stopTimes[i].ArrivalTime = scheduledArrivalTime.TimeOfDay;
                stopTimes[i].Delay = corresp.Arrival.Delay;
            }

            if (corresp.Departure != null && corresp.Departure.Delay != null)
            {
                var scheduledDepartureTime = DateTime.Today;
                scheduledDepartureTime = scheduledDepartureTime.AddHours(stopTimes[i].DepartureTime.Hours);
                scheduledDepartureTime = scheduledDepartureTime.AddMinutes(stopTimes[i].DepartureTime.Minutes);
                scheduledDepartureTime = scheduledDepartureTime.AddSeconds(stopTimes[i].DepartureTime.Seconds);
                scheduledDepartureTime = scheduledDepartureTime.AddSeconds((double)corresp.Departure.Delay);
                stopTimes[i].DepartureTime = scheduledDepartureTime.TimeOfDay;
                stopTimes[i].Delay = corresp.Departure.Delay;
            }
        }

        return stopTimes; 
    }

    private async Task<List<StopTimeDTO>> QueryStopStopTimes(string mode, string stopName, DateTime time)
    {
        var dayOfWeek = time.DayOfWeek;

        var query = _db.Stops
            .Where(s => s.Name.Contains(stopName))
            .Where(s => s.Mode == mode)
            .Join(_db.StopTimes, s => s.Id, st => st.StopId, (s, st) => new { s, st })
            .Where(x => x.st.PickupType == 0 || x.st.DropOffType == 0)
            .Join(_db.Trips, x => x.st.TripId, t => t.Id, (x, t) => new { x.s, x.st, t })
            .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.s, x.st, x.t, c })
            .Where(x => x.c.StartDate.Date <= time.Date && x.c.EndDate.Date >= time.Date)
            .Where(x =>
                (dayOfWeek == DayOfWeek.Monday && x.c.Monday == 1) ||
                (dayOfWeek == DayOfWeek.Tuesday && x.c.Tuesday == 1) ||
                (dayOfWeek == DayOfWeek.Wednesday && x.c.Wednesday == 1) ||
                (dayOfWeek == DayOfWeek.Thursday && x.c.Thursday == 1) ||
                (dayOfWeek == DayOfWeek.Friday && x.c.Friday == 1) ||
                (dayOfWeek == DayOfWeek.Saturday && x.c.Saturday == 1) ||
                (dayOfWeek == DayOfWeek.Sunday && x.c.Sunday == 1)
            )
            .Select(x => new
            {
                x.st.TripId,
                x.st.ArrivalTime,
                x.st.DepartureTime,
                x.t.RouteId,
                x.st.StopSequence,
                x.st.StopHeadSign,
                x.st.PickupType,
                x.st.DropOffType,
                x.st.ShapeDistanceTravelled
            });

        var missingHeadsigns = await query
            .Where(x => string.IsNullOrEmpty(x.StopHeadSign))
            .Select(x => x.TripId)
            .ToHashSetAsync();

        var missingHeadsignsDict = await GetHeadsigns(missingHeadsigns) ?? [];

        var stopTimes = await query
            .Select(x => new StopTimeDTO
            {
                TripId = x.TripId,
                ArrivalTime = x.ArrivalTime,
                DepartureTime = x.DepartureTime,
                RouteId = x.RouteId,
                StopSequence = x.StopSequence,
                StopHeadSign = string.IsNullOrWhiteSpace(x.StopHeadSign) ? missingHeadsignsDict.GetValueOrDefault(x.TripId) : x.StopHeadSign,
                PickupType = x.PickupType,
                DropOffType = x.DropOffType,
                ShapeDistanceTravelled = x.ShapeDistanceTravelled,
            })
            .ToListAsync();

        return stopTimes;
    }

    private async Task<List<StopTimeDTO>> QueryTripStopTimes(string mode, string tripId, DateTime time)
    {
        var dayOfWeek = time.DayOfWeek;

        var query = _db.StopTimes
            .Where(st => st.Mode == mode)
            .Where(st => st.TripId == tripId)
            .Where(st => st.PickupType == 0 || st.DropOffType == 0)
            .Join(_db.Trips, st => st.TripId, t => t.Id, (st, t) => new { st, t })
            .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.st, x.t, c })
            .Where(x => x.c.StartDate.Date <= time.Date && x.c.EndDate.Date >= time.Date)
            .Where(x =>
                (dayOfWeek == DayOfWeek.Monday && x.c.Monday == 1) ||
                (dayOfWeek == DayOfWeek.Tuesday && x.c.Tuesday == 1) ||
                (dayOfWeek == DayOfWeek.Wednesday && x.c.Wednesday == 1) ||
                (dayOfWeek == DayOfWeek.Thursday && x.c.Thursday == 1) ||
                (dayOfWeek == DayOfWeek.Friday && x.c.Friday == 1) ||
                (dayOfWeek == DayOfWeek.Saturday && x.c.Saturday == 1) ||
                (dayOfWeek == DayOfWeek.Sunday && x.c.Sunday == 1)
            )
            .Join(_db.Stops, x => x.st.StopId, s => s.Id, (x, s) => new { x.st, x.t, x.c, s })
            .Select(x => new
            {
                x.st.TripId,
                x.st.ArrivalTime,
                x.st.DepartureTime,
                x.s.Name,
                x.s.Id,
                x.t.RouteId,
                x.st.StopSequence,
                x.st.StopHeadSign,
                x.st.PickupType,
                x.st.DropOffType,
                x.st.ShapeDistanceTravelled
            });

        var missingHeadsigns = await query
            .Where(x => string.IsNullOrEmpty(x.StopHeadSign))
            .Select(x => x.TripId)
            .ToHashSetAsync();

        var missingHeadsignsDict = await GetHeadsigns(missingHeadsigns) ?? [];

        var stopTimes = await query
            .Select(x => new StopTimeDTO
            {
                TripId = x.TripId,
                ArrivalTime = x.ArrivalTime,
                DepartureTime = x.DepartureTime,
                StopName = x.Name,
                StopId = x.Id,
                StopSequence = x.StopSequence,
                StopHeadSign = string.IsNullOrWhiteSpace(x.StopHeadSign) ? missingHeadsignsDict.GetValueOrDefault(x.TripId) : x.StopHeadSign,
                PickupType = x.PickupType,
                DropOffType = x.DropOffType,
                ShapeDistanceTravelled = x.ShapeDistanceTravelled,
            })
            .ToListAsync();

        return stopTimes;
    }

    private static async Task<List<StopTimeDTO>> SortStopTimes(List<StopTimeDTO> stopTimes, DateTime time)
    {
        stopTimes.Sort((a, b) =>
        {
            bool aIsEarly = a.DepartureTime.Hours < 4;
            bool bIsEarly = b.DepartureTime.Hours < 4;

            if (aIsEarly == bIsEarly)
                return a.DepartureTime.CompareTo(b.DepartureTime);

            return aIsEarly ? 1 : -1;
        });

        return stopTimes;
    }

    private static async Task<List<StopTimeDTO>> SliceStopTimes(List<StopTimeDTO> stopTimes, DateTime time, bool before)
    {
        stopTimes.Sort((a, b) => a.DepartureTime.CompareTo(b.DepartureTime));
        var rotatedFirstIndex = stopTimes.FindIndex((st) => st.DepartureTime.Hours >= 4);

        var index = 0;
        for (var i = 0; i < stopTimes.Count; i++)
        {
            if (stopTimes[i].DepartureTime > time.TimeOfDay)
            {
                index = i;
                break;
            }
        }

        stopTimes.Sort((a, b) => 
        {
            bool aIsEarly = a.DepartureTime.Hours < 4;
            bool bIsEarly = b.DepartureTime.Hours < 4;
            
            if (aIsEarly == bIsEarly)
                return a.DepartureTime.CompareTo(b.DepartureTime);

            return aIsEarly ? 1 : -1;
        });

        if (index <= rotatedFirstIndex)
            index = stopTimes.Count - (rotatedFirstIndex - index);
        else
            index -= rotatedFirstIndex;

        var futureCount = 24;
        var startIndex = index;
        var endIndex = Math.Min(index + futureCount, stopTimes.Count);
        if (before) {
            var priorCount = 12;
            startIndex = Math.Max(0, index - priorCount);
            endIndex = index - 1;
        }

        var slicedStopTimes = stopTimes
            .Skip(startIndex)
            .Take(endIndex - startIndex)
            .ToList();

        return slicedStopTimes;
    }

    private async Task<Dictionary<string, string>> GetHeadsigns(HashSet<string> tripIds)
    {
        return await _db.Trips
            .Where(t => tripIds.Contains(t.Id.ToString()))
            .ToDictionaryAsync(
                t => t.Id.ToString(),
                t => t.HeadSign
            );
    }
}
