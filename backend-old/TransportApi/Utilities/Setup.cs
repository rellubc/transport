using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.IO.Compression;

using TransportStatic.Data;
using TransportStatic.Models;

namespace TransportStatic.Utils;

public static class Setup
{
    async public static Task PopulateSydney(this WebApplication app, string version, string mode)
    {
        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var client = factory.CreateClient("TransportNSW");
        var response = await client.GetAsync($"https://api.transport.nsw.gov.au/{version}/gtfs/schedule/{mode}");

        if (!response.IsSuccessStatusCode)
        {
            app.Logger.LogWarning($"Failed to fetch data: {response.StatusCode}");
            return;
        }

        var zipBytes = await response.Content.ReadAsByteArrayAsync();
        using var memoryStream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        var existingAgencyIds = new HashSet<string>(db.Agencies.Select(a => a.Id));
        var existingCalendarIds = new HashSet<string>(db.Calendars.Select(c => c.ServiceId));
        var existingRouteIds = new HashSet<string>(db.Routes.Select(r => r.Id));
        var existingShapeIds = new HashSet<string>(db.Shapes.Select(s => s.Id + "|" + s.Sequence + "|" + s.Mode));
        var existingStopIds = new HashSet<string>(db.Stops.Select(s => s.Id + "|" + s.Mode));
        var existingStopTimeIds = new HashSet<string>(db.StopTimes.Select(s => s.TripId + "|" + s.StopSequence));
        var existingTripIds = new HashSet<string>(db.Trips.Select(t => t.Id));

        var existingNoteIds = new HashSet<string>(db.Notes.Select(n => n.Id));
        var existingOccupancyIds = new HashSet<string>(db.Occupancies.Select(o => o.TripId + "|" + o.StopSequence + "|" + o.StartDate));
        var existingVehicleBoardingIds = new HashSet<string>(db.VehicleBoardings.Select(vb => vb.VehicleCategoryId + "|" + vb.ChildSequence + "|" + "|" + vb.BoardingAreaId));
        var existingVehicleCategoryIds = new HashSet<string>(db.VehicleCategories.Select(vc => vc.VehicleCategoryId));
        var existingVehicleCouplingIds = new HashSet<string>(db.VehicleCouplings.Select(vc => vc.ParentId + "|" + vc.ChildId + "|" + vc.ChildSequence));

        var agencies = new List<Agency>();
        var calendars = new List<Models.Calendar>();
        var routes = new List<Models.Route>();
        var shapes = new List<Shape>();
        var stops = new List<Stop>();
        var stopTimes = new List<StopTime>();
        var trips = new List<Trip>();

        var notes = new List<Note>();
        var occupancies = new List<Occupancy>();
        var vehicleBoardings = new List<VehicleBoarding>();
        var vehicleCategories = new List<VehicleCategory>();
        var vehicleCouplings = new List<VehicleCoupling>();
        
        var modeKey = mode == "metro" ? "Metro" : "Rail";

        foreach (var entry in archive.Entries)
        {
            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);

            // skip header line
            Console.WriteLine($"Processing {entry.Name}, {await reader.ReadLineAsync()}");

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                using var parser = new TextFieldParser(new StringReader(line));
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");

                var cols = parser.ReadFields();
                if (cols == null) continue;

                try
                {
                    switch (entry.FullName)
                    {
                        case "agency.txt":
                        {
                            if (!existingAgencyIds.Contains(cols[0]))
                            {
                                agencies.Add(Agency.ParseColumns(mode, cols));
                            }
                            break;
                        }
                        case "calendar.txt":
                        {
                            if (!existingCalendarIds.Contains(cols[0]))
                            {
                                calendars.Add(Models.Calendar.ParseColumns(cols));
                            }
                            break;
                        }
                        case "calendar_dates.txt":
                        {
                            // currently empty
                            break;
                        }
                        case "notes.txt":
                        {
                            if (!existingNoteIds.Contains(cols[0]))
                            {
                                notes.Add(Note.ParseColumns(cols));
                            }
                            break;
                        }
                        case "routes.txt":
                        {
                            if (!existingRouteIds.Contains(cols[0]))
                            {
                                routes.Add(Models.Route.ParseColumns(mode, cols));
                            }
                            break;
                        }
                        case "shapes.txt":
                        {
                            if (mode == "metro")
                            {
                                if (!existingShapeIds.Contains("M1_" + cols[0] + "|" + int.Parse(cols[3]) + "|" + modeKey))
                                {
                                    shapes.Add(Shape.ParseColumns(mode, cols));
                                }
                            }
                            else
                            {
                                if (!existingShapeIds.Contains(cols[0] + "|" + int.Parse(cols[3]) + "|" + modeKey))
                                {
                                    shapes.Add(Shape.ParseColumns(mode, cols));
                                }
                            }

                            break;
                        }
                        case "stops.txt":
                        {
                            if (!existingStopIds.Contains(cols[0] + "|" + modeKey))
                            {
                                stops.Add(Stop.ParseColumns(mode, cols));
                            }
                            break;
                        }
                        case "stop_times.txt":
                        {
                            if (!existingStopTimeIds.Contains(cols[0] + "|" + int.Parse(cols[4])))
                            {
                                stopTimes.Add(StopTime.ParseColumns(mode, cols));
                            }
                            break;
                        }
                        case "trips.txt":
                        {
                            if (!existingTripIds.Contains(cols[2]))
                            {
                                trips.Add(Trip.ParseColumns(mode, cols));
                            }
                            break;
                        }
                        case "vehicle_categories.txt":
                        {
                            if (!existingVehicleCategoryIds.Contains(cols[0]))
                            {
                                vehicleCategories.Add(VehicleCategory.ParseColumns(cols));
                            }
                            break;    
                        }
                        case "vehicle_boardings.txt":
                        {
                            if (!existingVehicleBoardingIds.Contains(cols[0] + "|" + cols[1] + "|" + cols[2] + "|" + cols[3]))
                            {
                                vehicleBoardings.Add(VehicleBoarding.ParseColumns(cols));
                            }
                            break;    
                        }
                        case "vehicle_couplings.txt":
                        {
                            if (!existingVehicleCouplingIds.Contains(cols[0] + "|" + cols[1] + "|" + cols[2]))
                            {
                                vehicleCouplings.Add(VehicleCoupling.ParseColumns(cols));
                            }
                            break;    
                        }
                        case "occupancies.txt":
                        {
                            // to populate
                            break;  
                        }
                    }
                }
                catch (Exception ex)
                {
                    app.Logger.LogWarning("Failed to parse entry {Entry}: {Error}", entry.FullName, ex.Message);
                    continue;
                }

            }
        }

        app.Logger.LogInformation("Adding New Entries");

        var newAgencies = agencies
            .Where(a => !existingAgencyIds.Contains(a.Id))
            .ToList();

        var newCalendars = calendars
            .Where(c => !existingCalendarIds.Contains(c.ServiceId))
            .ToList();

        var newNotes = notes
            .Where(n => !existingNoteIds.Contains(n.Id))
            .ToList();

        var newVehicleCategories = vehicleCategories
            .Where(vc => !existingVehicleCategoryIds.Contains(vc.VehicleCategoryId))
            .ToList();

        var newShapes = shapes
            .Where(s => !existingShapeIds.Contains($"{s.Id}|{s.Sequence}|{modeKey}"))
            .ToList();

        var newStops = stops
            .Where(s => !existingStopIds.Contains($"{s.Id}|{modeKey}"))
            .ToList();

        var newRoutes = routes
            .Where(r => !existingRouteIds.Contains(r.Id))
            .ToList();

        var newTrips = trips
            .Where(t => !existingTripIds.Contains(t.Id))
            .ToList();

        var newStopTimes = stopTimes
            .Where(st => !existingStopTimeIds.Contains($"{st.TripId}|{st.StopSequence}"))
            .ToList();

        var newVehicleBoardings = vehicleBoardings
            .Where(vb => !existingVehicleBoardingIds.Contains($"{vb.VehicleCategoryId}|{vb.ChildSequence}|{vb.BoardingAreaId}"))
            .ToList();

        var newVehicleCouplings = vehicleCouplings
            .Where(vc => !existingVehicleCouplingIds.Contains($"{vc.ParentId}|{vc.ChildId}|{vc.ChildSequence}"))
            .ToList();

        var newOccupancies = occupancies
            .Where(o => !existingOccupancyIds.Contains($"{o.TripId}|{o.StopSequence}|{o.StartDate}"))
            .ToList();

        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        
        app.Logger.LogInformation("Adding New Agencies");
        db.Agencies.AddRange(newAgencies);

        app.Logger.LogInformation("Adding New Calendars");
        db.Calendars.AddRange(newCalendars);

        app.Logger.LogInformation("Adding New Notes");
        db.Notes.AddRange(newNotes);

        app.Logger.LogInformation("Adding New Vehicle Categories");
        db.VehicleCategories.AddRange(newVehicleCategories);

        app.Logger.LogInformation("Adding New Shapes");
        db.Shapes.AddRange(newShapes);

        app.Logger.LogInformation("Adding New Stops");
        db.Stops.AddRange(newStops);
        
        await db.SaveChangesAsync();

        app.Logger.LogInformation("Adding New Routes");
        db.Routes.AddRange(newRoutes);
        
        await db.SaveChangesAsync();
        
        app.Logger.LogInformation("Adding New Trips");
        db.Trips.AddRange(newTrips);
        
        await db.SaveChangesAsync();

        app.Logger.LogInformation("Adding New Stop Times");
        const int batchSize = 10000;
        foreach (var batch in newStopTimes.Chunk(batchSize))
        {
            db.StopTimes.AddRange(batch);
            await db.SaveChangesAsync();
        }

        app.Logger.LogInformation("Adding New Vehicle Boardings");
        db.VehicleBoardings.AddRange(newVehicleBoardings);
        
        app.Logger.LogInformation("Adding New Vehicle Couplings");
        db.VehicleCouplings.AddRange(newVehicleCouplings);
        
        app.Logger.LogInformation("Adding New Occupancies");
        db.Occupancies.AddRange(newOccupancies);

        await db.SaveChangesAsync();
        
        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        app.Logger.LogInformation("Complete");
    }
}
