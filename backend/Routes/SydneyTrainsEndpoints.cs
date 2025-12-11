using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Routes
{
    public static class SydneyTrainsEndpoints
    {
        public static void GetSydneyTrains(this IEndpointRouteBuilder app)
        {
            app.MapGet("/trains/sydney/agencies", async (TransportDbContext db) =>
            {
                var sydneyTrainsRoutes = await db.Agencies
                    .Where(a => a.AgencyName.StartsWith("NSW"))
                    .Distinct()
                    .ToListAsync();
                return Results.Ok(sydneyTrainsRoutes);
            })
            .WithName("GetSydneyTrainsStations")
            .WithOpenApi();
        }
    }
}
