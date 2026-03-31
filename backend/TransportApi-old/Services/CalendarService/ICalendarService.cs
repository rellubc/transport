using TransportApi.DTOs;

namespace TransportApi.Services;

public interface ICalendarService
{
    Task<List<CalendarDto>> GetCalendars();
    Task<CalendarDto?> GetCalendar(string serviceId);
}