using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;
using TransportApi.DTOs.Realtime;
using TransportApi.Services;

using TransitRealtime;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney/combined")]
public class SydneyCombinedControllers : ControllerBase
{
    private readonly TransportDbContext _db;
    private readonly ISydneyTrainsService _services;

    public SydneyCombinedControllers(TransportDbContext db, ISydneyTrainsService services)
    {
        _db = db;
        _services = services;
    }

    [HttpGet("stop-times")]
    public async Task<ActionResult<List<StopTimeDto>>> GetSydneyCombinedStopTimes(string stopName, string timeString, bool before)
    {
        var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
        var dayOfWeek = time.DayOfWeek;

        var isMonday    = dayOfWeek == DayOfWeek.Monday;
        var isTuesday   = dayOfWeek == DayOfWeek.Tuesday;
        var isWednesday = dayOfWeek == DayOfWeek.Wednesday;
        var isThursday  = dayOfWeek == DayOfWeek.Thursday;
        var isFriday    = dayOfWeek == DayOfWeek.Friday;
        var isSaturday  = dayOfWeek == DayOfWeek.Saturday;
        var isSunday    = dayOfWeek == DayOfWeek.Sunday;

        var query = _db.StopTimes
            .Where(st => (st.PickupType || st.DropOffType))
            .Join(_db.Trips, st => st.TripId, t => t.Id, (st, t) => new { st, t })
            .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.st, x.t, c })
            .Where(x => (x.c.StartDate <= time && x.c.EndDate >= time) || (x.c.StartDate.DayOfWeek == dayOfWeek))
            .Where(x =>
                (isMonday    && x.c.Monday)    ||
                (isTuesday   && x.c.Tuesday)   ||
                (isWednesday && x.c.Wednesday) ||
                (isThursday  && x.c.Thursday)  ||
                (isFriday    && x.c.Friday)    ||
                (isSaturday  && x.c.Saturday)  ||
                (isSunday    && x.c.Sunday)
            )
            .Join(_db.Stops, x => x.st.StopId, s => s.Id, (x, s) => new { x.st, x.t, x.c, s})
            .Where(x => x.s.Name.Contains(stopName))
            .Select(x => new 
            {
                x.st.TripId,
                x.st.ArrivalTime,
                x.st.DepartureTime,
                x.t.RouteId,
                x.st.StopHeadSign,
                x.st.PickupType,
                x.st.DropOffType,
                x.st.ShapeDistanceTravelled,
                x.st.Mode,
            });

        Console.WriteLine(query.ToList().Count);

        var missingHeadsigns = await query
            .Where(x => string.IsNullOrEmpty(x.StopHeadSign))
            .Select(x => x.TripId)
            .ToHashSetAsync();

        var missingHeadsignsDict = await GetCombinedTripHeadsigns(missingHeadsigns) ?? new Dictionary<string, string>();

        var stopTimesDto = query
            .Select(x => new StopTimeDto
            {
                TripId = x.TripId,
                ArrivalTime = x.ArrivalTime,
                DepartureTime = x.DepartureTime,
                RouteId = x.RouteId,
                StopHeadSign = !string.IsNullOrEmpty(x.StopHeadSign) ? x.StopHeadSign : missingHeadsignsDict.GetValueOrDefault(x.TripId),
                PickupType = x.PickupType,
                DropOffType = x.DropOffType,
                ShapeDistanceTravelled = x.ShapeDistanceTravelled,
                Mode = x.Mode
            })
            .ToList();

        stopTimesDto.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));
        var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.ArrivalTime.Hours >= 4);

        var index = 0;
        for (var i = 0; i < stopTimesDto.Count; i++)
        {
            if (stopTimesDto[i].ArrivalTime > time.TimeOfDay)
            {
                index = i;
                break;
            }
        }

        stopTimesDto.Sort((a, b) => 
        {
            bool aIsEarly = a.ArrivalTime.Hours < 4;
            bool bIsEarly = b.ArrivalTime.Hours < 4;
            
            if (aIsEarly == bIsEarly)
                return a.ArrivalTime.CompareTo(b.ArrivalTime);

            return aIsEarly ? 1 : -1;
        });

        if (index <= rotatedFirstIndex)
            index = stopTimesDto.Count - (rotatedFirstIndex - index);
        else
            index -= rotatedFirstIndex;

        var futureCount = 24;
        var startIndex = index;
        var endIndex = Math.Min(index + futureCount, stopTimesDto.Count);
        if (before) {
            var priorCount = 12;
            startIndex = Math.Max(0, index - priorCount);
            endIndex = index - 1;
        }

        var slicedStopTimes = stopTimesDto
            .Skip(startIndex)
            .Take(endIndex - startIndex)
            .ToList();

        return Ok(slicedStopTimes);
    }

    private async Task<Dictionary<string, string>> GetCombinedTripHeadsigns(HashSet<string> tripIds)
    {
        var tripHeadsigns = await _db.Trips
            .Where(t => tripIds.Contains(t.Id.ToString()))
            .Select(t => new { t.Id, t.HeadSign })
            .ToListAsync();

        var tripHeadsignsDict = tripHeadsigns.ToDictionary(
            x => x.Id.ToString(),
            x => x.HeadSign
        );

        return tripHeadsignsDict;
    }
}
