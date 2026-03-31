using Microsoft.AspNetCore.Mvc;

using TransportApi.DTOs;
using TransportApi.Services;

namespace TransportApi.Controllers;

[ApiController]
[Route("api/sydney")]
public class CalendarController(ICalendarService calendarService) : ControllerBase
{
    private readonly ICalendarService _calendarService = calendarService;

    [HttpGet("calendars")]
    public async Task<ActionResult<List<CalendarDto>>> GetCalendares()
    {
        var calendars = await _calendarService.GetCalendars();
        return Ok(calendars);
    }

    [HttpGet("calendars/{calendarId}")]
    public async Task<ActionResult<List<CalendarDto>>> GetCalendar(string calendarId)
    {
        var calendar = await _calendarService.GetCalendar(calendarId);

        if (calendar == null)
        {
            return NotFound();
        }

        return Ok(calendar);
    }
}