using TransportStatic.DTOs;

namespace TransportStatic.Services;

public interface ICalendarService
{
    Task<List<CalendarDTO>> GetCalendars();
    Task<CalendarDTO?> GetCalendar(string serviceId);
}