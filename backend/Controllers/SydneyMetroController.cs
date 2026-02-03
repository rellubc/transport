using Microsoft.EntityFrameworkCore;

using backend.Data;
using backend.Models;

namespace backend.Controllers;

public static class SydneyMetroController
{
    async public static Task<List<Agency>> GetSydneyMetroAgencies(TransportDbContext db)
    {
        var sydneyMetroAgencies = await db.Agencies
            .Distinct()
            .ToListAsync();

        return sydneyMetroAgencies;
    }
    
    async public static Task<List<Calendar>> GetSydneyMetroCalendars(TransportDbContext db)
    {
        var sydneyMetroCalendars = await db.Calendars
            .Distinct()
            .ToListAsync();

        return sydneyMetroCalendars;
    }
    
    async public static Task<List<CalendarDate>> GetSydneyMetroCalendarDates(TransportDbContext db)
    {
        var sydneyMetroCalendarDates = await db.CalendarDates
            .Distinct()
            .ToListAsync();

        return sydneyMetroCalendarDates;
    }
    
    async public static Task<List<Note>> GetSydneyMetroNotes(TransportDbContext db)
    {
        var sydneyMetroNotes = await db.Notes
            .Distinct()
            .ToListAsync();

        return sydneyMetroNotes;
    }
    
    async public static Task<List<Models.Route>> GetSydneyMetroRoutes(TransportDbContext db)
    {
        var sydneyMetroRoutes = await db.Routes
            .Distinct()
            .ToListAsync();

        return sydneyMetroRoutes;
    }
    
    async public static Task<List<Shape>> GetSydneyMetroShapes(TransportDbContext db)
    {
        var sydneyMetroShapes = await db.Shapes
            .Distinct()
            .ToListAsync();

        return sydneyMetroShapes;
    }
    
    async public static Task<List<Stop>> GetSydneyMetroStops(TransportDbContext db)
    {
        var sydneyMetroStops = await db.Stops
            .Distinct()
            .ToListAsync();

        return sydneyMetroStops;
    }
    
    async public static Task<List<StopTime>> GetSydneyMetroStopTimes(TransportDbContext db)
    {
        var sydneyMetroStopTimes = await db.StopTimes
            .Distinct()
            .ToListAsync();

        return sydneyMetroStopTimes;
    }

    async public static Task<List<Trip>> GetSydneyMetroTrips(TransportDbContext db)
    {
        var sydneyMetroTrips = await db.Trips
            .Distinct()
            .ToListAsync();

        return sydneyMetroTrips;
    }
}
