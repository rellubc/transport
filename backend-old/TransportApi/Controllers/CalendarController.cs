using Microsoft.AspNetCore.Mvc;

using TransportStatic.DTOs;
using TransportStatic.Services;

namespace TransportStatic.Controllers;

[ApiController]
[Route("api/sydney")]
public class CalendarController(ICalendarService calendarService) : ControllerBase
{
    private readonly ICalendarService _calendarService = calendarService;

    [HttpGet("calendars")]
    public async Task<ActionResult<List<CalendarDTO>>> GetCalendares()
    {
        var calendars = await _calendarService.GetCalendars();
        return Ok(calendars);
    }

    [HttpGet("calendars/{calendarId}")]
    public async Task<ActionResult<List<CalendarDTO>>> GetCalendar(string calendarId)
    {
        var calendar = await _calendarService.GetCalendar(calendarId);

        if (calendar == null)
        {
            return NotFound();
        }

        return Ok(calendar);
    }
}