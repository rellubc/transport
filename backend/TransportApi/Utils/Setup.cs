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
        var existingCalendarIds = new HashSet<int>(db.Calendars.Select(c => c.ServiceId));
        var existingCalendarDateIds = new HashSet<int>(db.CalendarDates.Select(cd => cd.ServiceId));
        var existingNoteIds = new HashSet<string>(db.Notes.Select(n => n.Id));
        var existingRouteIds = new HashSet<string>(db.Routes.Select(r => r.Id));
        var existingShapeIds = new HashSet<string>(db.Shapes.Select(s => s.Id + "|" + s.Sequence));
        var existingStopIds = new HashSet<int>(db.Stops.Select(s => s.Id));
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
            await reader.ReadLineAsync();

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
                        if (!existingCalendarIds.Contains(int.Parse(cols[0])))
                        {
                            var entity = new Models.Calendar
                            {
                                ServiceId = int.Parse(cols[0]),
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
                                Color = cols[6],
                                TextColor = cols[7],
                                Url = cols[8],
                            };
                            newRoutes.Add(entity);
                        }
                        break;
                    }
                    case "shapes.txt":
                    {
                        if (!existingShapeIds.Contains(int.Parse(cols[0]) + "|" + int.Parse(cols[3])))
                        {
                            // if (cols[4] == "\"\"") cols[4] = "0";

                            var entity = new Shape
                            {
                                Id = int.Parse(cols[0]),
                                Latitude = decimal.Parse(cols[1], CultureInfo.InvariantCulture),
                                Longitude = decimal.Parse(cols[2], CultureInfo.InvariantCulture),
                                Sequence = int.Parse(cols[3]),
                                DistanceTravelled = decimal.Parse(cols[4], CultureInfo.InvariantCulture),
                            };
                            newShapes.Add(entity);
                        }
                        break;
                    }
                    case "stops.txt":
                    {
                        if (!existingStopIds.Contains(int.Parse(cols[0])))
                        {
                            var entity = new Stop
                            {
                                Id = int.Parse(cols[0]),
                                Name = cols[1],
                                Latitude = decimal.Parse(cols[2], CultureInfo.InvariantCulture),
                                Longitude = decimal.Parse(cols[3], CultureInfo.InvariantCulture),
                                LocationType = cols[4] == "1" ? "Station" : "Platform",
                                ParentStationId = cols[5].Length == 0 ? null : int.Parse(cols[5]),
                                WheelchairBoarding = cols[6] == "1",
                                PlatformCode = cols[7].Length == 0 ? null : int.Parse(cols[7]),
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
                                StopId = int.Parse(cols[3]),
                                StopSequence = int.Parse(cols[4]),
                                StopHeadSign = cols[5],
                                PickupType = cols[6] == "1",
                                DropOffType = cols[7] == "1",
                                ShapeDistanceTravelled = decimal.Parse(cols[8], CultureInfo.InvariantCulture),
                                Timepoint = cols[9] == "1",
                                StopNote = cols[10],
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
                                ServiceId = int.Parse(cols[1]),
                                Id = cols[2],
                                ShapeId = int.Parse(cols[3]),
                                HeadSign = cols[4],
                                DirectionId = cols[5] == "1",
                                ShortName = cols[5],
                                BlockId = cols[7],
                                WheelchairAccessible = true,
                                TripNote = cols[8],
                                RouteDirection = cols[9],
                                BikesAllowed = true,
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

        if (agenciesCount != existingAgencyIds.Count + newAgencies.Count) {
            app.Logger.LogInformation("Agency table miscount");
        } else if (calendarsCount != existingCalendarIds.Count + newCalendars.Count) {
            app.Logger.LogInformation("Calendar table miscount");
        } else if (calendarDatesCount != existingCalendarDateIds.Count + newCalendarDates.Count) {
            app.Logger.LogInformation("CalendarDate table miscount");
        } else if (notesCount != existingNoteIds.Count + newNotes.Count) {
            app.Logger.LogInformation("Note table miscount");
        } else if (routesCount != existingRouteIds.Count + newRoutes.Count) {
            app.Logger.LogInformation("Route table miscount");
        } else if (shapesCount != existingShapeIds.Count + newShapes.Count) {
            app.Logger.LogInformation("Shape table miscount");
        } else if (stopsCount != existingStopIds.Count + newStops.Count) {
            app.Logger.LogInformation("Stop table miscount");
        } else if (stopTimesCount != existingStopTimeIds.Count + newStopTimes.Count) {
            app.Logger.LogInformation("StopTime table miscount");
        } else if (tripsCount != existingTripIds.Count + newTrips.Count) {
            app.Logger.LogInformation("Trip table miscount");
        }

        app.Logger.LogInformation("Check Complete");
    }
}
