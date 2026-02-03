using backend.Controllers;
using backend.Data;

namespace backend.Routes;

public static class SydneyMetroEndpoints
{
    public static void GetSydneyMetro(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sydney/metro/agencies", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroAgencies(db);
        })
        .WithName("GetSydneyMetroAgencies");

        app.MapGet("/sydney/metro/calendars", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroCalendars(db);
        })
        .WithName("GetSydneyMetroCalendars");

        app.MapGet("/sydney/metro/calendardates", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroCalendarDates(db);
        })
        .WithName("GetSydneyMetroCalendarDates");

        app.MapGet("/sydney/metro/notes", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroNotes(db);
        })
        .WithName("GetSydneyMetroNotes");

        app.MapGet("/sydney/metro/routes", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroRoutes(db);
        })
        .WithName("GetSydneyMetroRoutes");

        app.MapGet("/sydney/metro/shapes", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroShapes(db);
        })
        .WithName("GetSydneyMetroShapes");

        app.MapGet("/sydney/metro/stops", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroStops(db);
        })
        .WithName("GetSydneyMetroStops");

        app.MapGet("/sydney/metro/stoptimes", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroStopTimes(db);
        })
        .WithName("GetSydneyMetroStopTimes");

        app.MapGet("/sydney/metro/trips", async (TransportDbContext db) =>
        {
            return await SydneyMetroController.GetSydneyMetroTrips(db);
        })
        .WithName("GetSydneyMetroTrips");
    }
}
