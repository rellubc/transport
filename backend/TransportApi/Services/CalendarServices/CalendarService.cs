using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.DTOs;

namespace TransportApi.Services;

public class CalendarService(TransportDbContext db) : ICalendarService
{
    private readonly TransportDbContext _db = db;

    public async Task<List<CalendarDto>> GetCalendars()
    {
        var calendars = await _db.Calendars
            .Select(c => new CalendarDto
            {
                ServiceId = c.ServiceId,
                Monday = c.Monday,
                Tuesday = c.Tuesday,
                Wednesday = c.Wednesday,
                Thursday = c.Thursday,
                Friday = c.Friday,
                Saturday = c.Saturday,
                Sunday = c.Sunday,
                StartDate = c.StartDate,
                EndDate = c.EndDate
            })
            .ToListAsync();

        return calendars;
    }

    public async Task<CalendarDto?> GetCalendar(string serviceId)
    {
        var calendar = await _db.Calendars
            .Where(c => c.ServiceId == serviceId)
            .Select(c => new CalendarDto
            {
                ServiceId = c.ServiceId,
                Monday = c.Monday,
                Tuesday = c.Tuesday,
                Wednesday = c.Wednesday,
                Thursday = c.Thursday,
                Friday = c.Friday,
                Saturday = c.Saturday,
                Sunday = c.Sunday,
                StartDate = c.StartDate,
                EndDate = c.EndDate
            })
            .FirstOrDefaultAsync();

        return calendar;
    }
}