using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.IO.Compression;

using TransportApi.Data;
using TransportApi.Models;

namespace TransportApi.Utils;

public static class Setup
{
    async public static Task PopulateSydneyMetro(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var client = factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v2/gtfs/schedule/metro");

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
        var existingCalendarDateIds = new HashSet<string>(db.CalendarDates.Select(cd => cd.ServiceId));
        var existingNoteIds = new HashSet<string>(db.Notes.Select(n => n.Id));
        var existingRouteIds = new HashSet<string>(db.Routes.Select(r => r.Id));
        var existingShapeIds = new HashSet<string>(db.Shapes.Select(s => s.Id + "|" + s.Sequence + "|" + s.Mode));
        var existingStopIds = new HashSet<string>(db.Stops.Select(s => s.Id + "|" + s.Mode));
        var existingStopTimeIds = new HashSet<string>(db.StopTimes.Select(s => s.TripId + "|" + s.StopSequence));
        var existingTripIds = new HashSet<string>(db.Trips.Select(t => t.Id));

        var newAgencies = new List<Agency>();
        var newCalendars = new List<Models.Calendar>();
        var newCalendarDates = new List<CalendarDate>();
        var newNotes = new List<Note>();
        var newRoutes = new List<Models.Route>();
        var newShapes = new List<Shape>();
        var newStops = new List<Stop>();
        var newStopTimes = new List<StopTime>();
        var newTrips = new List<Trip>();

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
                                var entity = new Agency
                                {
                                    Id = cols[0],
                                    Name = cols[1],
                                    Url = cols[2],
                                    Timezone = cols[3],
                                    Lang = cols[4],
                                    Phone = cols[5],
                                    FareUrl = cols[6],
                                    Email = cols[7],
                                };
                                newAgencies.Add(entity);
                            }
                            break;
                        }
                        case "calendar.txt":
                        {
                            if (!existingCalendarIds.Contains(cols[0]))
                            {
                                var entity = new Models.Calendar
                                {
                                    ServiceId = cols[0],
                                    Monday = cols[1] == "1",
                                    Tuesday = cols[2] == "1",
                                    Wednesday = cols[3] == "1",
                                    Thursday = cols[4] == "1",
                                    Friday = cols[5] == "1",
                                    Saturday = cols[6] == "1",
                                    Sunday = cols[7] == "1",
                                    StartDate = DateTime.ParseExact(cols[8], "yyyyMMdd", CultureInfo.InvariantCulture),
                                    EndDate = DateTime.ParseExact(cols[9], "yyyyMMdd", CultureInfo.InvariantCulture),
                                };
                                newCalendars.Add(entity);
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
                                var entity = new Note
                                {
                                    Id = cols[0],
                                    Text = cols[1],
                                };
                                newNotes.Add(entity);
                            }
                            break;
                        }
                        case "routes.txt":
                        {
                            if (!existingRouteIds.Contains(cols[0]))
                            {
                                var entity = new Models.Route
                                {
                                    Id = cols[0],
                                    AgencyId = cols[1],
                                    ShortName = cols[2],
                                    LongName = cols[3],
                                    Description = cols[4],
                                    Type = int.Parse(cols[5]),
                                    Colour = cols[6],
                                    TextColour = cols[7],
                                    Url = cols[8],
                                };
                                newRoutes.Add(entity);
                            }
                            break;
                        }
                        case "shapes.txt":
                        {
                            if (!existingShapeIds.Contains(cols[0] + "|" + int.Parse(cols[3])  + "|metro"))
                            {
                                // if (cols[4] == "\"\"") cols[4] = "0";

                                var entity = new Shape
                                {
                                    Id = cols[0],
                                    Latitude = decimal.Parse(cols[1], CultureInfo.InvariantCulture),
                                    Longitude = decimal.Parse(cols[2], CultureInfo.InvariantCulture),
                                    Sequence = int.Parse(cols[3]),
                                    DistanceTravelled = decimal.Parse(cols[4], CultureInfo.InvariantCulture),
                                    Mode = "metro"
                                };
                                newShapes.Add(entity);
                            }
                            break;
                        }
                        case "stops.txt":
                        {
                            if (!existingStopIds.Contains(cols[0] + "|metro"))
                            {
                                var entity = new Stop
                                {
                                    Id = cols[0],
                                    Name = cols[1],
                                    Latitude = decimal.Parse(cols[2], CultureInfo.InvariantCulture),
                                    Longitude = decimal.Parse(cols[3], CultureInfo.InvariantCulture),
                                    LocationType = cols[4] == "1" ? "Station" : "Platform",
                                    ParentStationId = !string.IsNullOrEmpty(cols[5]) ? cols[5] : string.Empty,
                                    WheelchairBoarding = cols[6] == "1",
                                    PlatformCode = cols[7].Length == 0 ? null : int.Parse(cols[7]),
                                    Mode = "metro",
                                };
                                newStops.Add(entity);
                            }

                            break;
                        }
                        case "stop_times.txt":
                        {
                            if (!existingStopTimeIds.Contains(cols[0] + "|" + int.Parse(cols[4])))
                            {
                                // if (cols[8] == "\"\"") cols[8] = "0";

                                int hour1 = int.Parse(cols[1][..2]);
                                int hour2 = int.Parse(cols[2][..2]);
                                cols[1] = string.Concat((hour1 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[1].AsSpan(2));
                                cols[2] = string.Concat((hour2 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[2].AsSpan(2));

                                var entity = new StopTime
                                {
                                    TripId = cols[0],
                                    ArrivalTime = TimeSpan.ParseExact(cols[1], @"hh\:mm\:ss", CultureInfo.InvariantCulture),
                                    DepartureTime = TimeSpan.ParseExact(cols[2], @"hh\:mm\:ss", CultureInfo.InvariantCulture),
                                    StopId = cols[3],
                                    StopSequence = cols[4],
                                    StopHeadSign = cols[5],
                                    PickupType = cols[6] == "0",
                                    DropOffType = cols[7] == "0",
                                    ShapeDistanceTravelled = decimal.Parse(cols[8], CultureInfo.InvariantCulture),
                                    Timepoint = cols[9] == "1",
                                    StopNote = cols[10],
                                    Mode = "metro",
                                };
                                newStopTimes.Add(entity);
                            }
                            break;
                        }
                        case "trips.txt":
                        {
                            if (!existingTripIds.Contains(cols[2]))
                            {
                                var entity = new Trip
                                {
                                    RouteId = cols[0],
                                    ServiceId = cols[1],
                                    Id = cols[2],
                                    ShapeId = cols[3],
                                    HeadSign = cols[4],
                                    DirectionId = cols[5] == "1",
                                    ShortName = cols[6],
                                    BlockId = cols[7],
                                    WheelchairAccessible = cols[8] == "1",
                                    TripNote = !string.IsNullOrEmpty(cols[9]) ? cols[9] : string.Empty,
                                    RouteDirection = cols[10],
                                    BikesAllowed = cols[11] == "1",
                                };
                                newTrips.Add(entity);
                            }
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

        if (newAgencies.Count > 0)
            app.Logger.LogInformation("New Agencies: {Count}", newAgencies.Count);

        if (newCalendars.Count > 0)
            app.Logger.LogInformation("New Calendars: {Count}", newCalendars.Count);

        if (newCalendarDates.Count > 0)
            app.Logger.LogInformation("New CalendarDates: {Count}", newCalendarDates.Count);

        if (newNotes.Count > 0)
            app.Logger.LogInformation("New Notes: {Count}", newNotes.Count);

        if (newRoutes.Count > 0)
            app.Logger.LogInformation("New Routes: {Count}", newRoutes.Count);

        if (newShapes.Count > 0)
            app.Logger.LogInformation("New Shapes: {Count}", newShapes.Count);

        if (newStops.Count > 0)
            app.Logger.LogInformation("New Stops: {Count}", newStops.Count);

        if (newStopTimes.Count > 0)
            app.Logger.LogInformation("New StopTimes: {Count}", newStopTimes.Count);

        if (newTrips.Count > 0)
            app.Logger.LogInformation("New Trips: {Count}", newTrips.Count);

        app.Logger.LogInformation("Adding New Entries");
        db.Agencies.AddRange(newAgencies);
        await db.SaveChangesAsync();

        db.Calendars.AddRange(newCalendars);
        await db.SaveChangesAsync();

        db.CalendarDates.AddRange(newCalendarDates);
        await db.SaveChangesAsync();

        db.Notes.AddRange(newNotes);
        await db.SaveChangesAsync();

        db.Routes.AddRange(newRoutes);
        await db.SaveChangesAsync();

        db.Shapes.AddRange(newShapes);
        await db.SaveChangesAsync();

        db.Stops.AddRange(newStops);
        await db.SaveChangesAsync();

        db.Trips.AddRange(newTrips);
        await db.SaveChangesAsync();

        db.StopTimes.AddRange(newStopTimes);
        await db.SaveChangesAsync();

        app.Logger.LogInformation("Checking new entries");

        var agenciesCount = await db.Agencies.CountAsync();
        var calendarsCount = await db.Calendars.CountAsync();
        var calendarDatesCount = await db.CalendarDates.CountAsync();
        var notesCount = await db.Notes.CountAsync();
        var routesCount = await db.Routes.CountAsync();
        var shapesCount = await db.Shapes.CountAsync();
        var stopsCount = await db.Stops.CountAsync();
        var stopTimesCount = await db.StopTimes.CountAsync();
        var tripsCount = await db.Trips.CountAsync();

        if (agenciesCount != existingAgencyIds.Count + newAgencies.Count)
            app.Logger.LogInformation("Agency table miscount");
        if (calendarsCount != existingCalendarIds.Count + newCalendars.Count)
            app.Logger.LogInformation("Calendar table miscount");
        if (calendarDatesCount != existingCalendarDateIds.Count + newCalendarDates.Count)
            app.Logger.LogInformation("CalendarDate table miscount");
        if (notesCount != existingNoteIds.Count + newNotes.Count)
            app.Logger.LogInformation("Note table miscount");
        if (routesCount != existingRouteIds.Count + newRoutes.Count)
            app.Logger.LogInformation("Route table miscount");
        if (shapesCount != existingShapeIds.Count + newShapes.Count)
            app.Logger.LogInformation("Shape table miscount");
        if (stopsCount != existingStopIds.Count + newStops.Count)
            app.Logger.LogInformation("Stop table miscount");
        if (stopTimesCount != existingStopTimeIds.Count + newStopTimes.Count)
            app.Logger.LogInformation("StopTime table miscount");
        if (tripsCount != existingTripIds.Count + newTrips.Count)
            app.Logger.LogInformation("Trip table miscount");

        app.Logger.LogInformation("Check Complete");
    }

    // TODO: Add remaining fields for some entries
    async public static Task PopulateSydneyTrains(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var client = factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v1/gtfs/schedule/sydneytrains");

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
        var existingCalendarDateIds = new HashSet<string>(db.CalendarDates.Select(cd => cd.ServiceId));
        var existingNoteIds = new HashSet<string>(db.Notes.Select(n => n.Id));
        var existingRouteIds = new HashSet<string>(db.Routes.Select(r => r.Id));
        var existingShapeIds = new HashSet<string>(db.Shapes.Select(s => s.Id + "|" + s.Sequence + "|" + s.Mode));
        var existingStopIds = new HashSet<string>(db.Stops.Select(s => s.Id + "|" + s.Mode));
        var existingStopTimeIds = new HashSet<string>(db.StopTimes.Select(s => s.TripId + "|" + s.StopSequence));
        var existingTripIds = new HashSet<string>(db.Trips.Select(t => t.Id));
        var existingVehicleCategoryIds = new HashSet<string>(db.VehicleCategories.Select(vc => vc.VehicleCategoryId));
        var existingVehicleBoardingIds = new HashSet<string>(db.VehicleBoardings.Select(vb => vb.VehicleCategoryId + "|" + vb.ChildSequence + "|" + vb.GrandchildSequence + "|" + vb.BoardingAreaId));
        var existingVehicleCouplingIds = new HashSet<string>(db.VehicleCouplings.Select(vc => vc.ParentId + "|" + vc.ChildId + "|" + vc.ChildSequence));
        var existingOccupancyIds = new HashSet<string>(db.Occupancies.Select(o => o.TripId + "|" + o.StopSequence));

        var newAgencies = new List<Agency>();
        var newCalendars = new List<Models.Calendar>();
        var newCalendarDates = new List<CalendarDate>();
        var newNotes = new List<Note>();
        var newRoutes = new List<Models.Route>();
        var newShapes = new List<Shape>();
        var newStops = new List<Stop>();
        var newStopTimes = new List<StopTime>();
        var newTrips = new List<Trip>();
        var newVehicleCategories = new List<VehicleCategory>();
        var newVehicleBoardings = new List<VehicleBoarding>();
        var newVehicleCouplings = new List<VehicleCoupling>();
        var newOccupancies = new List<Occupancy>();

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
                                var entity = new Agency
                                {
                                    Id = cols[0],
                                    Name = cols[1],
                                    Url = cols[2],
                                    Timezone = cols[3],
                                    Lang = cols[4],
                                    Phone = cols[5],
                                };
                                newAgencies.Add(entity);
                            }
                            break;
                        }
                        case "calendar.txt":
                        {
                            if (!existingCalendarIds.Contains(cols[0]))
                            {
                                var entity = new Models.Calendar
                                {
                                    ServiceId = cols[0],
                                    Monday = cols[1] == "1",
                                    Tuesday = cols[2] == "1",
                                    Wednesday = cols[3] == "1",
                                    Thursday = cols[4] == "1",
                                    Friday = cols[5] == "1",
                                    Saturday = cols[6] == "1",
                                    Sunday = cols[7] == "1",
                                    StartDate = DateTime.ParseExact(cols[8], "yyyyMMdd", CultureInfo.InvariantCulture),
                                    EndDate = DateTime.ParseExact(cols[9], "yyyyMMdd", CultureInfo.InvariantCulture),
                                };
                                newCalendars.Add(entity);
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
                                var entity = new Note
                                {
                                    Id = cols[0],
                                    Text = cols[1],
                                };
                                newNotes.Add(entity);
                            }
                            break;
                        }
                        case "routes.txt":
                        {
                            if (!existingRouteIds.Contains(cols[0]))
                            {
                                var entity = new Models.Route
                                {
                                    Id = cols[0],
                                    AgencyId = cols[1],
                                    ShortName = cols[2],
                                    LongName = cols[3],
                                    Description = cols[4],
                                    Type = int.Parse(cols[5]),
                                    Url = cols[6],
                                    Colour = cols[7],
                                    TextColour = cols[8],
                                };
                                newRoutes.Add(entity);
                            }
                            break;
                        }
                        case "shapes.txt":
                        {
                            if (!existingShapeIds.Contains(cols[0] + "|" + int.Parse(cols[3]) + "|sydneytrains"))
                            {
                                // if (cols[4] == "\"\"") cols[4] = "0";

                                var entity = new Shape
                                {
                                    Id = cols[0],
                                    Latitude = decimal.Parse(cols[1], CultureInfo.InvariantCulture),
                                    Longitude = decimal.Parse(cols[2], CultureInfo.InvariantCulture),
                                    Sequence = int.Parse(cols[3]),
                                    DistanceTravelled = cols[4] == "" ? 0 : decimal.Parse(cols[4], CultureInfo.InvariantCulture),
                                    Mode = "sydneytrains"
                                };
                                newShapes.Add(entity);
                            }
                            break;
                        }
                        case "stops.txt":
                        {
                            if (!existingStopIds.Contains(cols[0] + "|sydneytrains"))
                            {
                                var entity = new Stop
                                {
                                    Id = cols[0],
                                    Code = cols[1],
                                    Name = cols[2],
                                    Description = !string.IsNullOrEmpty(cols[3]) ? cols[3] : string.Empty,
                                    Latitude = decimal.Parse(cols[4], CultureInfo.InvariantCulture),
                                    Longitude = decimal.Parse(cols[5], CultureInfo.InvariantCulture),
                                    ZoneId = !string.IsNullOrEmpty(cols[6]) ? cols[6] : string.Empty,
                                    Url = !string.IsNullOrEmpty(cols[7]) ? cols[7] : string.Empty,
                                    LocationType = cols[8] == "1" ? "Station" : "Platform",
                                    ParentStationId = !string.IsNullOrEmpty(cols[9]) ? cols[9] : string.Empty,
                                    Timezone = !string.IsNullOrEmpty(cols[10]) ? cols[10] : string.Empty,
                                    WheelchairBoarding = cols[11] == "1",
                                    Mode = "sydneytrains",
                                    Network = Constants.MetropolitanStations.Any(s => cols[1].StartsWith(s, StringComparison.OrdinalIgnoreCase)) ? "metropolitan" : "regional",
                                };
                                newStops.Add(entity);
                            }
                            break;
                        }
                        case "stop_times.txt":
                        {
                            if (!existingStopTimeIds.Contains(cols[0] + "|" + int.Parse(cols[4])))
                            {
                                // if (cols[8] == "\"\"") cols[8] = "0";

                                int hour1 = int.Parse(cols[1][..2]);
                                int hour2 = int.Parse(cols[2][..2]);
                                cols[1] = string.Concat((hour1 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[1].AsSpan(2));
                                cols[2] = string.Concat((hour2 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[2].AsSpan(2));

                                var entity = new StopTime
                                {
                                    TripId = cols[0],
                                    ArrivalTime = TimeSpan.ParseExact(cols[1], @"hh\:mm\:ss", CultureInfo.InvariantCulture),
                                    DepartureTime = TimeSpan.ParseExact(cols[2], @"hh\:mm\:ss", CultureInfo.InvariantCulture),
                                    StopId = cols[3],
                                    StopSequence = cols[4],
                                    StopHeadSign = !string.IsNullOrEmpty(cols[5]) ? cols[5] : string.Empty,
                                    PickupType = cols[6] == "0",
                                    DropOffType = cols[7] == "0",
                                    ShapeDistanceTravelled = !string.IsNullOrEmpty(cols[8]) ? decimal.Parse(cols[8], CultureInfo.InvariantCulture) : 0,
                                    Mode = "sydneytrains",
                                };
                                newStopTimes.Add(entity);
                            }
                            break;
                        }
                        case "trips.txt":
                        {
                            if (!existingTripIds.Contains(cols[2]))
                            {

                                // RTTA_REV 1978.102.130 Y907.1978.102.130.D.10.87900439 Empty Train  0 312168 RTTA_REV 0 D10
                                // route_id","service_id","trip_id","trip_headsign","trip_short_name","direction_id","block_id","shape_id","wheelchair_accessible","vehicle_category_id
                                var entity = new Trip
                                {
                                    RouteId = cols[0],
                                    ServiceId = cols[1],
                                    Id = cols[2],
                                    HeadSign = cols[3],
                                    ShortName = !string.IsNullOrEmpty(cols[4]) ? cols[4] : string.Empty,
                                    DirectionId = cols[5] == "1",
                                    BlockId = cols[6],
                                    ShapeId = cols[7],
                                    WheelchairAccessible = cols[8] == "1",
                                    VehicleCategoryId = cols[9],
                                };
                                newTrips.Add(entity);
                            }
                            break;
                        }
                        case "vehicle_categories.txt":
                        {
                            if (!existingVehicleCategoryIds.Contains(cols[0]))
                            {
                                var entity = new VehicleCategory
                                {
                                    VehicleCategoryId = cols[0],
                                    VehicleCategoryName = cols[1],
                                };
                                newVehicleCategories.Add(entity);
                            }
                            break;    
                        }
                        case "vehicle_boardings.txt":
                        {
                            if (!existingVehicleBoardingIds.Contains(cols[0] + "|" + cols[1] + "|" + cols[2] + "|" + cols[3]))
                            {
                                var entity = new VehicleBoarding
                                {
                                    VehicleCategoryId = cols[0],
                                    ChildSequence = cols[1],
                                    GrandchildSequence = cols[2],
                                    BoardingAreaId = cols[3],
                                };
                                newVehicleBoardings.Add(entity);
                            }
                            break;    
                        }
                        case "vehicle_couplings.txt":
                        {
                            if (!existingVehicleCouplingIds.Contains(cols[0] + "|" + cols[1] + "|" + cols[2]))
                            {
                                var entity = new VehicleCoupling
                                {
                                    ParentId = cols[0],
                                    ChildId = cols[1],
                                    ChildSequence = int.Parse(cols[2]),
                                    ChildLabel = cols[3],
                                };
                                newVehicleCouplings.Add(entity);
                            }
                            break;    
                        }

                        case "occupancies.txt":
                        {
                            if (!existingOccupancyIds.Contains(cols[0] + "|" + cols[1]))
                            {
                                var entity = new Occupancy
                                {
                                    TripId = cols[0],
                                    StopSequence = cols[1],
                                    OccupancyStatus = int.Parse(cols[2]),
                                    Monday = cols[3] == "1",
                                    Tuesday = cols[4] == "1",
                                    Wednesday = cols[5] == "1",
                                    Thursday = cols[6] == "1",
                                    Friday = cols[7] == "1",
                                    Saturday = cols[8] == "1",
                                    Sunday = cols[9] == "1",
                                    StartDate = DateTime.ParseExact(cols[10], "yyyyMMdd", CultureInfo.InvariantCulture),
                                    EndDate = !string.IsNullOrEmpty(cols[11]) ? DateTime.ParseExact(cols[11], "yyyyMMdd", CultureInfo.InvariantCulture) : new DateTime(),
                                    Exception = cols[12] == "1",
                                };
                                newOccupancies.Add(entity);
                            }
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

        if (newAgencies.Count > 0)
            app.Logger.LogInformation("New Agencies: {newAgencies.Count}", newAgencies.Count);
        if (newCalendars.Count > 0)
            app.Logger.LogInformation("New Calendars: {Count}", newCalendars.Count);
        if (newCalendarDates.Count > 0)
            app.Logger.LogInformation("New CalendarDates: {Count}", newCalendarDates.Count);
        if (newNotes.Count > 0)
            app.Logger.LogInformation("New Notes: {Count}", newNotes.Count);
        if (newRoutes.Count > 0)
            app.Logger.LogInformation("New Routes: {Count}", newRoutes.Count);
        if (newShapes.Count > 0)
            app.Logger.LogInformation("New Shapes: {Count}", newShapes.Count);
        if (newStops.Count > 0)
            app.Logger.LogInformation("New Stops: {Count}", newStops.Count);
        if (newStopTimes.Count > 0)
            app.Logger.LogInformation("New StopTimes: {Count}", newStopTimes.Count);
        if (newTrips.Count > 0)
            app.Logger.LogInformation("New Trips: {Count}", newTrips.Count);
        if (newVehicleCategories.Count > 0)
            app.Logger.LogInformation("New VehicleCategories: {Count}", newVehicleCategories.Count);
        if (newVehicleBoardings.Count > 0)
            app.Logger.LogInformation("New VehicleBoardings: {Count}", newVehicleBoardings.Count);
        if (newVehicleCouplings.Count > 0)
            app.Logger.LogInformation("New VehicleCouplings: {Count}", newVehicleCouplings.Count);
        if (newOccupancies.Count > 0)
            app.Logger.LogInformation("New Occupancies: {Count}", newOccupancies.Count);

        app.Logger.LogInformation("Adding New Entries");
        db.Agencies.AddRange(newAgencies);
        await db.SaveChangesAsync();

        db.Calendars.AddRange(newCalendars);
        await db.SaveChangesAsync();

        db.CalendarDates.AddRange(newCalendarDates);
        await db.SaveChangesAsync();

        db.Notes.AddRange(newNotes);
        await db.SaveChangesAsync();

        db.Routes.AddRange(newRoutes);
        await db.SaveChangesAsync();

        db.Shapes.AddRange(newShapes);
        await db.SaveChangesAsync();

        db.Stops.AddRange(newStops);
        await db.SaveChangesAsync();

        db.Trips.AddRange(newTrips);
        await db.SaveChangesAsync();

        db.StopTimes.AddRange(newStopTimes);
        await db.SaveChangesAsync();

        db.VehicleCategories.AddRange(newVehicleCategories);
        await db.SaveChangesAsync();

        db.VehicleCouplings.AddRange(newVehicleCouplings);
        await db.SaveChangesAsync();

        db.VehicleBoardings.AddRange(newVehicleBoardings);
        await db.SaveChangesAsync();

        db.Occupancies.AddRange(newOccupancies);
        await db.SaveChangesAsync();

        app.Logger.LogInformation("Checking new entries");

        var agenciesCount = await db.Agencies.CountAsync();
        var calendarsCount = await db.Calendars.CountAsync();
        var calendarDatesCount = await db.CalendarDates.CountAsync();
        var notesCount = await db.Notes.CountAsync();
        var routesCount = await db.Routes.CountAsync();
        var shapesCount = await db.Shapes.CountAsync();
        var stopsCount = await db.Stops.CountAsync();
        var stopTimesCount = await db.StopTimes.CountAsync();
        var tripsCount = await db.Trips.CountAsync();
        var vehicleCategoriesCount = await db.VehicleCategories.CountAsync();
        var vehicleBoardingsCount = await db.VehicleBoardings.CountAsync();
        var vehicleCouplingsCount = await db.VehicleCouplings.CountAsync();
        var occupanciesCount = await db.Occupancies.CountAsync();

        if (agenciesCount != existingAgencyIds.Count + newAgencies.Count)
            app.Logger.LogInformation("Agency table miscount");
        if (calendarsCount != existingCalendarIds.Count + newCalendars.Count)
            app.Logger.LogInformation("Calendar table miscount");
        if (calendarDatesCount != existingCalendarDateIds.Count + newCalendarDates.Count)
            app.Logger.LogInformation("CalendarDate table miscount");
        if (notesCount != existingNoteIds.Count + newNotes.Count)
            app.Logger.LogInformation("Note table miscount");
        if (routesCount != existingRouteIds.Count + newRoutes.Count)
            app.Logger.LogInformation("Route table miscount");
        if (shapesCount != existingShapeIds.Count + newShapes.Count)
            app.Logger.LogInformation("Shape table miscount");
        if (stopsCount != existingStopIds.Count + newStops.Count)
            app.Logger.LogInformation("Stop table miscount");
        if (stopTimesCount != existingStopTimeIds.Count + newStopTimes.Count)
            app.Logger.LogInformation("StopTime table miscount");
        if (tripsCount != existingTripIds.Count + newTrips.Count)
            app.Logger.LogInformation("Trip table miscount");
        if (vehicleBoardingsCount != existingVehicleCategoryIds.Count + newVehicleBoardings.Count)
            app.Logger.LogInformation("New VehicleCategories: {Count}", newVehicleCategories.Count);
        if (vehicleBoardingsCount != existingVehicleBoardingIds.Count + newVehicleBoardings.Count)
            app.Logger.LogInformation("New VehicleBoardings: {Count}", newVehicleBoardings.Count);
        if (vehicleCouplingsCount != existingVehicleCouplingIds.Count + newVehicleCouplings.Count)
            app.Logger.LogInformation("New VehicleCouplings: {Count}", newVehicleCouplings.Count);
        if (occupanciesCount != existingOccupancyIds.Count + newOccupancies.Count)
            app.Logger.LogInformation("New Occupancies: {Count}", newOccupancies.Count);

        app.Logger.LogInformation("Check Complete");
    }
}
