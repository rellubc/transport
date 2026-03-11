// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// using TransportApi.Data;
// using TransportApi.DTOs;
// using TransportApi.DTOs.Realtime;
// using TransportApi.Services;

// using TransitRealtime;

// namespace TransportApi.Controllers;

// [ApiController]
// [Route("api/sydney/combined")]
// public class SydneyCombinedControllers : ControllerBase
// {
//     private readonly TransportDbContext _db;
//     private readonly ISydneyTrainsService _services;

//     public SydneyCombinedControllers(TransportDbContext db, ISydneyTrainsService services)
//     {
//         _db = db;
//         _services = services;
//     }

//     [HttpGet("stop-times")]
//     public async Task<ActionResult<List<StopTimeDto>>> GetSydneyCombinedStopTimes(string stopId)
//     {
//         var today = DateTime.UtcNow.Date;
//         var dayOfWeek = today.DayOfWeek;

//         var stopIds: string[] = [];



//         var query = _db.StopTimes
//             .Where(st => st.StopId == stopId)
//             .Join(
//                 _db.Trips,
//                 st => st.TripId,
//                 t => t.Id,
//                 (st, t) => new { st, t }
//             )
//             .Join(
//                 _db.Calendars,
//                 x => x.t.ServiceId,
//                 c => c.ServiceId,
//                 (x, c) => new { x.st, x.t, c }
//             )
//             .Where(x => x.c.StartDate <= today && x.c.EndDate >= today);

//         query = dayOfWeek switch
//         {
//             DayOfWeek.Monday    => query.Where(x => x.c.Monday),
//             DayOfWeek.Tuesday   => query.Where(x => x.c.Tuesday),
//             DayOfWeek.Wednesday => query.Where(x => x.c.Wednesday),
//             DayOfWeek.Thursday  => query.Where(x => x.c.Thursday),
//             DayOfWeek.Friday    => query.Where(x => x.c.Friday),
//             DayOfWeek.Saturday  => query.Where(x => x.c.Saturday),
//             DayOfWeek.Sunday    => query.Where(x => x.c.Sunday),
//             _ => query
//         };

//         var stopTimesDto = query
//             .Join(
//                 _db.Stops,
//                 x => x.st.StopId,
//                 s => s.Id,
//                 (x, s) => new StopTimeDto
//                 {
//                     TripId = x.st.TripId,
//                     ArrivalTime = x.st.ArrivalTime,
//                     DepartureTime = x.st.DepartureTime,
//                     StopId = x.st.StopId,
//                     StopName = s.Name,
//                     RouteId = x.t.RouteId,
//                     StopSequence = x.st.StopSequence,
//                     StopHeadSign = x.st.StopHeadSign ?? string.Empty,
//                     PickupType = x.st.PickupType,
//                     DropOffType = x.st.DropOffType,
//                     ShapeDistanceTravelled = x.st.ShapeDistanceTravelled,
//                     TimePoint = x.st.Timepoint ?? null,
//                     StopNote = x.st.StopNote ?? null,
//                     Mode = x.st.Mode,
//                 })
//             .ToList();

//         return Ok(stopTimesDto);
//     }
// }
