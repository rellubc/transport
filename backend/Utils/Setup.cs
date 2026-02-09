using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.IO.Compression;

using backend.Data;
using backend.Models;

namespace backend.Utils;

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
            Console.WriteLine($"Failed to fetch data: {response.StatusCode}");
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

        Console.WriteLine("******************************************");
        Console.WriteLine("           Checking New Entries           ");
        Console.WriteLine("******************************************");
        foreach (var entry in archive.Entries)
        {
            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            
            string? line;
            line = await reader.ReadLineAsync();
            
            while ((line = await reader.ReadLineAsync()) != null)
            {
                using var parser = new TextFieldParser(new StringReader(line));
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");

                var cols = parser.ReadFields();

                if (cols == null) return;

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
                                Latitude = decimal.Parse(cols[1]),
                                Longitude = decimal.Parse(cols[2]),
                                Sequence = int.Parse(cols[3]),
                                DistanceTraveled = decimal.Parse(cols[4]),
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
                                Latitude = decimal.Parse(cols[2]),
                                Longitude = decimal.Parse(cols[3]),
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
                                ArrivalTime = TimeSpan.ParseExact(cols[1], "HH:mm:ss", CultureInfo.InvariantCulture),
                                DepartureTime = TimeSpan.ParseExact(cols[2], "HH:mm:ss", CultureInfo.InvariantCulture),
                                StopId = int.Parse(cols[3]),
                                StopSequence = int.Parse(cols[4]),
                                StopHeadSign = cols[5],
                                PickupType = cols[6] == "1",
                                DropOffType = cols[7] == "1",
                                ShapeDistanceTraveled = decimal.Parse(cols[8]),
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
        }

        if (newAgencies.Count > 0)
            Console.WriteLine("New Agencies: " + newAgencies.Count);

        if (newCalendars.Count > 0)
            Console.WriteLine("New Calendars: " + newCalendars.Count);

        if (newCalendarDates.Count > 0)
            Console.WriteLine("New CalendarDates: " + newCalendarDates.Count);

        if (newNotes.Count > 0)
            Console.WriteLine("New Notes: " + newNotes.Count);

        if (newRoutes.Count > 0)
            Console.WriteLine("New Routes: " + newRoutes.Count);

        if (newShapes.Count > 0)
            Console.WriteLine("New Shapes: " + newShapes.Count);

        if (newStops.Count > 0)
            Console.WriteLine("New Stops: " + newStops.Count);

        if (newStopTimes.Count > 0)
            Console.WriteLine("New StopTimes: " + newStopTimes.Count);

        if (newTrips.Count > 0)
            Console.WriteLine("New Trips: " + newTrips.Count);

        Console.WriteLine("******************************************");
        Console.WriteLine("            Adding New Entries            ");
        Console.WriteLine("******************************************");
        db.Agencies.AddRange(newAgencies);
        db.SaveChanges();

        db.Calendars.AddRange(newCalendars);
        db.SaveChanges();

        db.CalendarDates.AddRange(newCalendarDates);
        db.SaveChanges();

        db.Notes.AddRange(newNotes);
        db.SaveChanges();

        db.Routes.AddRange(newRoutes);
        db.SaveChanges();

        db.Shapes.AddRange(newShapes);
        db.SaveChanges();

        db.Stops.AddRange(newStops);
        db.SaveChanges();
        
        db.Trips.AddRange(newTrips);
        db.SaveChanges();

        db.StopTimes.AddRange(newStopTimes);
        db.SaveChanges();

        Console.WriteLine("******************************************");
        Console.WriteLine("                 Checking                 ");
        Console.WriteLine("******************************************");
        if (db.Agencies.Count() != existingAgencyIds.Count() + newAgencies.Count()) {
            Console.WriteLine("Agency table miscount");
        } else if (db.Calendars.Count() != existingCalendarIds.Count() + newCalendars.Count()) {
            Console.WriteLine("Calendar table miscount");
        } else if (db.CalendarDates.Count() != existingCalendarDateIds.Count() + newCalendarDates.Count()) {
            Console.WriteLine("CalendarDate table miscount");
        } else if (db.Notes.Count() != existingNoteIds.Count() + newNotes.Count()) {
            Console.WriteLine("Note table miscount");
        } else if (db.Routes.Count() != existingRouteIds.Count() + newRoutes.Count()) {
            Console.WriteLine("Route table miscount");
        } else if (db.Shapes.Count() != existingShapeIds.Count() + newShapes.Count()) {
            Console.WriteLine("Shape table miscount");
        } else if (db.Stops.Count() != existingStopIds.Count() + newStops.Count()) {
            Console.WriteLine("Stop table miscount");
        } else if (db.StopTimes.Count() != existingStopTimeIds.Count() + newStopTimes.Count()) {
            Console.WriteLine("StopTime table miscount");
        } else if (db.Trips.Count() != existingTripIds.Count() + newTrips.Count()) {
            Console.WriteLine("Trip table miscount");
        }

        Console.WriteLine("******************************************");
        Console.WriteLine("              Check Complete              ");
        Console.WriteLine("******************************************");
    }

    async private static Task CountLines(this ZipArchiveEntry entry)
    {
        using var entryStreamCount = entry.Open();
        using var readerCount = new StreamReader(entryStreamCount);

        string? line_counting;
        int count = 0;
        while((line_counting = await readerCount.ReadLineAsync()) != null)
        {
            count++;   
        }

        Console.WriteLine(entry.FullName + "|" + count);
    }
}
