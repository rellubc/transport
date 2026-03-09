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
//     public async Task<ActionResult<List<CalendarDto>>> GetSydneyTrainsCalendars()
//     {
//         var calendars = await _db.Calendars.ToListAsync();

//         var calendarsDto = calendars
//         .Select(c => new CalendarDto
//         {
//             ServiceId = c.ServiceId,
//             Monday = c.Monday,
//             Tuesday = c.Tuesday,
//             Wednesday = c.Wednesday,
//             Thursday = c.Thursday,
//             Friday = c.Friday,
//             Saturday = c.Saturday,
//             Sunday = c.Sunday,
//             StartDate = c.StartDate,
//             EndDate = c.EndDate
//         }).ToList();

//         return Ok(calendarsDto);
//     }

//     [HttpGet("calendar-dates")]
//     public async Task<ActionResult<List<CalendarDateDto>>> GetSydneyTrainsCalendarDates() =>
//         Ok(await _db.CalendarDates.ToListAsync());

//     [HttpGet("notes")]
//     public async Task<ActionResult<List<NoteDto>>> GetSydneyTrainsNotes() =>
//         Ok(await _db.Notes.ToListAsync());

//     [HttpGet("routes")]
//     public async Task<ActionResult<List<DTOs.RouteDto>>> GetSydneyTrainsRoutes() => 
//         Ok(await _db.Routes.ToListAsync());

//     [HttpGet("shapes")]
//     public async Task<ActionResult<Dictionary<int, List<ShapeDetails>>>> GetSydneyTrainsShapes() {
//         var shapes = await _db.Shapes.ToListAsync();

//         Dictionary<string, List<ShapeDetails>> shapesDictionary = [];

//         foreach (var shape in shapes)
//         {
//             if (shape.Mode != "sydneytrains") continue;
//             if (!shapesDictionary.TryGetValue(shape.Id, out List<ShapeDetails>? value))
//             {
//                 value = [];
//                 shapesDictionary[shape.Id] = value;
//             }

//             value.Add(new ShapeDetails
//             {
//                 Latitude = shape.Latitude,
//                 Longitude = shape.Longitude,
//                 Sequence = shape.Sequence,
//                 DistanceTravelled = shape.DistanceTravelled,
//                 Mode = shape.Mode,
//             });
//         }

//         foreach (var shapeId in shapesDictionary.Keys)
//         {
//             shapesDictionary[shapeId] = [.. shapesDictionary[shapeId].OrderBy(s => s.Sequence)];
//         }

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
//         }).ToList();

//         return Ok(stopsDto);
//     }

//     [HttpGet("stop-times")]
//     public async Task<ActionResult<List<StopTimeDto>>> GetSydneyTrainsStopTimes(string stopId)
//     {
//         var stopTimes = await _db.StopTimes.Where(st => st.StopId == stopId).ToListAsync();

//         var stopTimesDto = stopTimes
//         .Where(st => st.StopId == stopId)
//         .Select(st => new StopTimeDto
//         {
//             TripId = st.TripId,
//             ArrivalTime = st.ArrivalTime,
//             DepartureTime = st.DepartureTime,
//             StopId = st.StopId,
//             StopSequence = st.StopSequence,
//             StopHeadSign = st.StopHeadSign,
//             PickupType = st.PickupType,
//             DropOffType = st.DropOffType,
//             ShapeDistanceTravelled = st.ShapeDistanceTravelled,
//             Timepoint = st.Timepoint,
//             StopNote = st.StopNote,
//         }).ToList();

//         return Ok(stopTimesDto);
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
//             ShortName = trip.ShortName,
//             BlockId = trip.BlockId,
//             WheelchairAccessible = trip.WheelchairAccessible,
//             TripNote = trip.TripNote,
//             RouteDirection = trip.RouteDirection,
//             BikesAllowed = trip.BikesAllowed,
//         };

//         return Ok(tripDto);
//     }

//     [HttpGet("realtime-trip-updates")]
//     public async Task<ActionResult<TripUpdateDto>> GetSydneyTrainsRealtimeStopTimes(string tripId)
//     {
//         var tripUpdate = await _services.TrainsRealtimeTripUpdates(tripId);

//         return Ok(tripUpdate);
//     }

//     [HttpGet("realtime-vehicle-positions")]
//     public async Task<ActionResult<List<VehiclePositionDto>>> GetSydneyTrainsRealtimeVehiclePositions()
//     {
//         var vehiclePositions = await _services.TrainsRealtimeVehiclePositions();

//         return Ok(vehiclePositions);
//     }
// }
