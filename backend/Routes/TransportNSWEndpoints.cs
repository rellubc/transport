using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Routes
{
    public static class TransportNSWEndpoints
    {
        public static void GetTransportNSWTest(this IEndpointRouteBuilder app)
        {
            app.MapGet("/tfnsw/agencies", async (TransportDbContext db) =>
            {
                var agencies = await db.Agencies.ToListAsync();
                return Results.Ok(agencies);
            })
            .WithName("GetTransportNSWAgencies")
            .WithOpenApi();

            app.MapGet("/tfnsw/agencies/pt", async (TransportDbContext db) =>
            {
                var agencies = await db.Agencies
                    .Where(a => a.AgencyName.StartsWith("Sydney") || a.AgencyName.StartsWith("NSW"))
                    .ToListAsync();
                return Results.Ok(agencies);
            })
            .WithName("GetTransportNSWPTAgencies")
            .WithOpenApi();

            app.MapGet("/tfnsw/calendars", async (TransportDbContext db) =>
            {
                var calendars = await db.Calendars.ToListAsync();
                return Results.Ok(calendars);
            })
            .WithName("GetTransportNSWCalendars")
            .WithOpenApi();

            app.MapGet("/tfnsw/routes", async (TransportDbContext db) =>
            {
                var routes = await db.Routes.ToListAsync();
                return Results.Ok(routes);
            })
            .WithName("GetTransportNSWRoutes")
            .WithOpenApi();

            app.MapGet("/tfnsw/stops", async (TransportDbContext db) =>
            {
                var stops = await db.Stops.ToListAsync();
                return Results.Ok(stops);
            })
            .WithName("GetTransportNSWStops")
            .WithOpenApi();

            app.MapGet("/tfnsw/stoptimes", async (TransportDbContext db) =>
            {
                var stoptimes = await db.StopTimes.ToListAsync();
                return Results.Ok(stoptimes);
            })
            .WithName("GetTransportNSWStoptimes")
            .WithOpenApi();

            app.MapGet("/tfnsw/shapes", async (TransportDbContext db) =>
            {
                var shapes = await db.Shapes.ToListAsync();
                return Results.Ok(shapes);
            })
            .WithName("GetTransportNSWShapes")
            .WithOpenApi();

            app.MapGet("/tfnsw/trips", async (TransportDbContext db) =>
            {
                var trips = await db.Trips.ToListAsync();
                return Results.Ok(trips);
            })
            .WithName("GetTransportNSWTrips")
            .WithOpenApi();
        }
    }
}
