using Microsoft.EntityFrameworkCore;

using TransportStatic.Data;
using TransportStatic.DTOs;

namespace TransportStatic.Services;

public class CalendarService(TransportDbContext db) : ICalendarService
{
    private readonly TransportDbContext _db = db;

    public async Task<List<CalendarDTO>> GetCalendars()
    {
        var calendars = await _db.Calendars
            .Select(c => new CalendarDTO
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

    public async Task<CalendarDTO?> GetCalendar(string serviceId)
    {
        var calendar = await _db.Calendars
            .Where(c => c.ServiceId == serviceId)
            .Select(c => new CalendarDTO
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