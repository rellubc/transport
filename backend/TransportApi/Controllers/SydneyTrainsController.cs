// using System.Globalization;

// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// using TransportApi.Data;
// using TransportApi.DTOs;
// using TransportApi.DTOs.Realtime;
// using TransportApi.Services;

// using TransitRealtime;

// namespace TransportApi.Controllers;

// [ApiController]
// [Route("api/sydney/trains")]
// public class SydneyTrainsController : ControllerBase
// {
//     private readonly TransportDbContext _db;
//     private readonly ISydneyTrainsService _services;

//     public SydneyTrainsController(TransportDbContext db, ISydneyTrainsService services)
//     {
//         _db = db;
//         _services = services;
//     }

//     [HttpGet("agencies")]
//     public async Task<ActionResult<List<AgencyDto>>> GetSydneyTrainsAgencies() =>
//         Ok(await _db.Agencies.ToListAsync());

//     [HttpGet("calendars")]
//     public async Task<ActionResult<List<CalendarDto>>> GetSydneyTrainsCalendars(string serviceId)
//     {
//         var calendar = await _db.Calendars.Where(c => c.ServiceId == serviceId).SingleOrDefaultAsync();

//         // invalid serviceId
//         if (calendar == null)
//         {
//             return NotFound();
//         }

//         var calendarDto = new CalendarDto
//         {
//             ServiceId = calendar.ServiceId,
//             Monday = calendar.Monday,
//             Tuesday = calendar.Tuesday,
//             Wednesday = calendar.Wednesday,
//             Thursday = calendar.Thursday,
//             Friday = calendar.Friday,
//             Saturday = calendar.Saturday,
//             Sunday = calendar.Sunday,
//             StartDate = calendar.StartDate,
//             EndDate = calendar.EndDate
//         };

//         return Ok(calendarDto);
//     }

//     [HttpGet("notes")]
//     public async Task<ActionResult<List<NoteDto>>> GetSydneyTrainsNotes() =>
//         Ok(await _db.Notes.ToListAsync());

//     [HttpGet("routes")]
//     public async Task<ActionResult<List<DTOs.RouteDto>>> GetSydneyTrainsRoutes() => 
//         Ok(await _db.Routes.ToListAsync());

//     [HttpGet("shapes")]
//     public async Task<ActionResult<Dictionary<int, List<ShapeDetails>>>> GetSydneyTrainsShapes() {

//         var shapesDictionary = await _db.Shapes
//             .AsNoTracking()
//             .Where(s => s.Mode == "sydneytrains")
//             .GroupBy(s => s.Id)
//             .ToDictionaryAsync(
//                 x => x.Key,
//                 x => x.OrderBy(s => s.Sequence)
//                     .Select(s => new ShapeDetails
//                     {
//                         Latitude = s.Latitude,
//                         Longitude = s.Longitude,
//                         Sequence = s.Sequence,
//                         Mode = s.Mode,
//                     })
//                     .ToList()
//             );

//         return Ok(shapesDictionary);
//     }

//     [HttpGet("stops")]
//     public async Task<ActionResult<List<StopDto>>> GetSydneyTrainsStops() {
//         var stops = await _db.Stops.ToListAsync();

//         var stopsDto = stops
//         .Where(s => s.Mode == "sydneytrains")
//         .Select(s => new StopDto
//         {
//             Id = s.Id,
//             Code = s.Code,
//             Name = s.Name,
//             Description = s.Description,
//             Latitude = s.Latitude,
//             Longitude = s.Longitude,
//             ZoneId = s.ZoneId,
//             Url = s.Url,
//             LocationType = s.LocationType,
//             ParentStationId = s.ParentStationId,
//             Timezone = s.Timezone,
//             WheelchairBoarding = s.WheelchairBoarding,
//             Mode = s.Mode,
//             Network = s.Network,
//         }).ToList();

//         return Ok(stopsDto);
//     }

//     [HttpGet("stops-platforms")]
//     public async Task<ActionResult<List<StopDto>>> GetSydneyTrainsStopsPlatforms(string stopId) {
//         var stops = await _db.Stops.ToListAsync();

//         var stopsDto = stops
//         .Where(s => s.Mode == "sydneytrains" && s.ParentStationId == stopId)
//         .Select(s => new StopDto
//         {
//             Id = s.Id,
//             Code = s.Code,
//             Name = s.Name,
//             Description = s.Description,
//             Latitude = s.Latitude,
//             Longitude = s.Longitude,
//             ZoneId = s.ZoneId,
//             Url = s.Url,
//             LocationType = s.LocationType,
//             ParentStationId = s.ParentStationId,
//             Timezone = s.Timezone,
//             WheelchairBoarding = s.WheelchairBoarding,
//             Mode = s.Mode,
//         }).ToList();

//         return Ok(stopsDto);
//     }

//     [HttpGet("stop-times")]
//     public async Task<ActionResult<List<StopTimeDto>>> GetSydneyTrainsStopTimes(string stopName, string timeString, bool before)
//     {
//         var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
//         var dayOfWeek = time.DayOfWeek;

//         var query = _db.StopTimes
//             .Where(st => st.Mode == "sydneytrains" && (st.PickupType || st.DropOffType))
//             .Join(_db.Trips, st => st.TripId, t => t.Id, (st, t) => new { st, t })
//             .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.st, x.t, c })
//             .Where(x => (x.c.StartDate <= time && x.c.EndDate >= time) || (x.c.StartDate.DayOfWeek == dayOfWeek))
//             .Where(x =>
//                 (dayOfWeek == DayOfWeek.Monday    && x.c.Monday)    ||
//                 (dayOfWeek == DayOfWeek.Tuesday   && x.c.Tuesday)   ||
//                 (dayOfWeek == DayOfWeek.Wednesday && x.c.Wednesday) ||
//                 (dayOfWeek == DayOfWeek.Thursday  && x.c.Thursday)  ||
//                 (dayOfWeek == DayOfWeek.Friday    && x.c.Friday)    ||
//                 (dayOfWeek == DayOfWeek.Saturday  && x.c.Saturday)  ||
//                 (dayOfWeek == DayOfWeek.Sunday    && x.c.Sunday)
//             )
//             .Join(_db.Stops, x => x.st.StopId, s => s.Id, (x, s) => new { x.st, x.t, x.c, s})
//             .Where(x => x.s.Name.Contains(stopName))
//             .Select(x => new 
//             {
//                 x.st.TripId,
//                 x.st.ArrivalTime,
//                 x.st.DepartureTime,
//                 x.t.RouteId,
//                 x.st.StopHeadSign,
//                 x.st.PickupType,
//                 x.st.DropOffType,
//                 x.st.ShapeDistanceTravelled,
//                 x.st.Mode,
//             });

//         var missingHeadsigns = await query
//             .Where(x => string.IsNullOrEmpty(x.StopHeadSign))
//             .Select(x => x.TripId)
//             .ToHashSetAsync();

//         var missingHeadsignsDict = await GetSydneyTrainsTripHeadsigns(missingHeadsigns) ?? new Dictionary<string, string>();

//         var stopTimesDto = query
//             .Select(x => new StopTimeDto
//             {
//                 TripId = x.TripId,
//                 ArrivalTime = x.ArrivalTime,
//                 DepartureTime = x.DepartureTime,
//                 RouteId = x.RouteId,
//                 StopHeadSign = !string.IsNullOrEmpty(x.StopHeadSign) ? x.StopHeadSign : missingHeadsignsDict.GetValueOrDefault(x.TripId),
//                 PickupType = x.PickupType,
//                 DropOffType = x.DropOffType,
//                 ShapeDistanceTravelled = x.ShapeDistanceTravelled,
//                 Mode = x.Mode
//             })
//             .ToList();

//         stopTimesDto.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));
//         var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.ArrivalTime.Hours >= 4);

//         var index = 0;
//         for (var i = 0; i < stopTimesDto.Count; i++)
//         {
//             if (stopTimesDto[i].ArrivalTime > time.TimeOfDay)
//             {
//                 index = i;
//                 break;
//             }
//         }

//         stopTimesDto.Sort((a, b) => 
//         {
//             bool aIsEarly = a.ArrivalTime.Hours < 4;
//             bool bIsEarly = b.ArrivalTime.Hours < 4;
            
//             if (aIsEarly == bIsEarly)
//                 return a.ArrivalTime.CompareTo(b.ArrivalTime);

//             return aIsEarly ? 1 : -1;
//         });

//         if (index <= rotatedFirstIndex)
//             index = stopTimesDto.Count - (rotatedFirstIndex - index);
//         else
//             index -= rotatedFirstIndex;

//         var futureCount = 24;
//         var startIndex = index;
//         var endIndex = Math.Min(index + futureCount, stopTimesDto.Count);
//         if (before) {
//             var priorCount = 12;
//             startIndex = Math.Max(0, index - priorCount);
//             endIndex = index - 1;
//         }

//         var slicedStopTimes = stopTimesDto
//             .Skip(startIndex)
//             .Take(endIndex - startIndex)
//             .ToList();

//         return Ok(slicedStopTimes);
//     }

//     private async Task<Dictionary<string, string>> GetSydneyTrainsTripHeadsigns(HashSet<string> tripIds)
//     {
//         var tripHeadsigns = await _db.Trips
//             .Where(t => tripIds.Contains(t.Id.ToString()))
//             .Select(t => new { t.Id, t.HeadSign })
//             .ToListAsync();

//         var tripHeadsignsDict = tripHeadsigns.ToDictionary(
//             x => x.Id.ToString(),
//             x => x.HeadSign
//         );

//         return tripHeadsignsDict;
//     }

//     [HttpGet("trips")]
//     public async Task<ActionResult<TripDto>> GetSydneyTrainsTrips(string tripId)
//     {
//         var trip = await _db.Trips.Where(t => t.Id == tripId).SingleOrDefaultAsync();

//         // invalid tripId
//         if (trip == null)
//         {
//             return NotFound();
//         }

//         var tripDto = new TripDto
//         {
//             Id = trip.Id,
//             RouteId = trip.RouteId,
//             ServiceId = trip.ServiceId,
//             ShapeId = trip.ShapeId,
//             HeadSign = trip.HeadSign,
//             DirectionId = trip.DirectionId,
//             ShortName = trip.ShortName ?? string.Empty,
//             BlockId = trip.BlockId,
//             WheelchairAccessible = trip.WheelchairAccessible,
//             VehicleCategoryId = trip.VehicleCategoryId,
//         };

//         return Ok(tripDto);
//     }

//     [HttpGet("trip-stop-times")]
//     public async Task<ActionResult<TripDto>> GetSydneyTrainsTripStopTimes(string tripId, string timeString)
//     {
//         var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(timeString).ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
//         var dayOfWeek = time.DayOfWeek;

//         var query = _db.StopTimes
//             .Where(st => st.TripId == tripId && st.Mode == "sydneytrains" && (st.PickupType || st.DropOffType))
//             .Join(_db.Trips, st => st.TripId, t => t.Id, (st, t) => new { st, t })
//             .Join(_db.Calendars, x => x.t.ServiceId, c => c.ServiceId, (x, c) => new { x.st, x.t, c })
//             .Where(x => (x.c.StartDate <= time && x.c.EndDate >= time) || (x.c.StartDate.DayOfWeek == dayOfWeek))
//             .Where(x =>
//                 (dayOfWeek == DayOfWeek.Monday && x.c.Monday) ||
//                 (dayOfWeek == DayOfWeek.Tuesday && x.c.Tuesday) ||
//                 (dayOfWeek == DayOfWeek.Wednesday && x.c.Wednesday) ||
//                 (dayOfWeek == DayOfWeek.Thursday && x.c.Thursday) ||
//                 (dayOfWeek == DayOfWeek.Friday && x.c.Friday) ||
//                 (dayOfWeek == DayOfWeek.Saturday && x.c.Saturday) ||
//                 (dayOfWeek == DayOfWeek.Sunday && x.c.Sunday)
//             )
//             .Join(_db.Stops, x => x.st.StopId, s => s.Id, (x, s) => new { x.st, x.t, x.c, s})
//             .Select(x => new 
//             {
//                 x.st.TripId,
//                 x.st.ArrivalTime,
//                 x.st.DepartureTime,
//                 x.s.Name,
//                 x.s.Id,
//                 x.t.RouteId,
//                 x.st.StopSequence,
//                 x.st.StopHeadSign,
//                 x.st.PickupType,
//                 x.st.DropOffType,
//                 x.st.ShapeDistanceTravelled,
//                 x.st.Mode,
//             });

//         var missingHeadsigns = await query
//             .Where(x => string.IsNullOrEmpty(x.StopHeadSign))
//             .Select(x => x.TripId)
//             .ToHashSetAsync();

//         var missingHeadsignsDict = await GetSydneyTrainsTripHeadsigns(missingHeadsigns) ?? new Dictionary<string, string>();

//         var stopTimesDto = query
//             .Select(x => new StopTimeDto
//             {
//                 TripId = x.TripId,
//                 ArrivalTime = x.ArrivalTime,
//                 DepartureTime = x.DepartureTime,
//                 StopName = x.Name,
//                 StopId = x.Id,
//                 RouteId = x.RouteId,
//                 StopSequence = x.StopSequence,
//                 StopHeadSign = !string.IsNullOrEmpty(x.StopHeadSign) ? x.StopHeadSign : missingHeadsignsDict.GetValueOrDefault(x.TripId),
//                 PickupType = x.PickupType,
//                 DropOffType = x.DropOffType,
//                 ShapeDistanceTravelled = x.ShapeDistanceTravelled,
//                 Mode = x.Mode
//             })
//             .ToList();

//         stopTimesDto.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));
//         var rotatedFirstIndex = stopTimesDto.FindIndex((st) => st.ArrivalTime.Hours >= 4);

//         var index = 0;
//         for (var i = 0; i < stopTimesDto.Count; i++)
//         {
//             if (stopTimesDto[i].ArrivalTime > time.TimeOfDay)
//             {
//                 index = i;
//                 break;
//             }
//         }

//         stopTimesDto.Sort((a, b) => 
//         {
//             bool aIsEarly = a.ArrivalTime.Hours < 4;
//             bool bIsEarly = b.ArrivalTime.Hours < 4;
            
//             if (aIsEarly == bIsEarly)
//                 return a.ArrivalTime.CompareTo(b.ArrivalTime);

//             return aIsEarly ? 1 : -1;
//         });

//         return Ok(stopTimesDto);
//     }

//     [HttpGet("realtime-trip-updates")]
//     public async Task<ActionResult<TripUpdateDto>> GetSydneyTrainsRealtimeStopTimes(string tripId)
//     {
//         var tripUpdate = await _services.SydneyTrainsRealtimeTripUpdates(tripId);

//         return Ok(tripUpdate);
//     }

//     [HttpGet("realtime-vehicle-positions")]
//     public async Task<ActionResult<List<VehiclePositionDto>>> GetSydneyTrainsRealtimeVehiclePositions()
//     {
//         var vehiclePositions = await _services.SydneyTrainsRealtimeVehiclePositions();

//         return Ok(vehiclePositions);
//     }
// }
