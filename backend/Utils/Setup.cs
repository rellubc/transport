using System.Globalization;
using System.IO.Compression;
using backend.Data;
using Microsoft.EntityFrameworkCore.Metadata;

namespace backend.Utils
{
    public static class Setup
    {
        async public static Task Populate(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

            var client = factory.CreateClient("TransportNSW");
            var response = await client.GetAsync("https://api.transport.nsw.gov.au/v1/gtfs/schedule/sydneytrains");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch data: {response.StatusCode}");
                return;
            }

            var zipBytes = await response.Content.ReadAsByteArrayAsync();
            using var memoryStream = new MemoryStream(zipBytes);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            var newAgencies = new List<Agency>();
            var newCalendars = new List<Data.Calendar>();
            var newRoutes = new List<Data.Route>();
            var newShapes = new List<Shape>();
            var newStops = new List<Stop>();
            var newStopTimes = new List<StopTime>();
            var newTrips = new List<Trip>();
            var newVehicleBoardings = new List<VehicleBoarding>();
            var newVehicleCategories = new List<VehicleCategory>();
            var newVehicleCouplings = new List<VehicleCoupling>();

            var existingAgencyIds = new HashSet<String>(db.Agencies.Select(a => a.AgencyId));
            var existingCalendarIds = new HashSet<String>(db.Calendars.Select(c => c.ServiceId));
            var existingRouteIds = new HashSet<String>(db.Routes.Select(r => r.RouteId));
            var existingShapeIds = new HashSet<String>(db.Shapes.Select(s => s.ShapeId + "|" + s.ShapePtSequence));
            var existingStopIds = new HashSet<String>(db.Stops.Select(s => s.StopId));
            var existingStopTimeIds = new HashSet<String>(db.StopTimes.Select(s => s.TripId + "|" + s.StopSequence));
            var existingTripIds = new HashSet<String>(db.Trips.Select(t => t.TripId));
            var existingVehicleBoardingIds = new HashSet<String>(db.VehicleBoardings.Select(vb => vb.VehicleCategoryId + "|" + vb.ChildSequence + "|" + vb.GrandchildSequence + "|" + vb.BoardingAreaId));
            var existingVehicleCategoryIds = new HashSet<String>(db.VehicleCategories.Select(vc => vc.VehicleCategoryId));
            var existingVehicleCouplingIds = new HashSet<String>(db.VehicleCouplings.Select(vc => vc.ParentId + "|" + vc.ChildId + "|" + vc.ChildSequence));

            foreach (var file in archive.Entries)
            {
                using var fileStreamCount = file.Open();
                using var readerCount = new StreamReader(fileStreamCount);
                string? line_counting;
                int count = 0;
                while((line_counting = await readerCount.ReadLineAsync()) != null)
                {
                    count++;   
                }

                Console.WriteLine(file.FullName + "|" + count);


                using var fileStream = file.Open();
                using var reader = new StreamReader(fileStream);
                
                string? line;
                line = await reader.ReadLineAsync();
                
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var cols = line.Split(',');
                    switch (file.FullName)
                    {
                        case "agency.txt":
                        {
                            if (!existingAgencyIds.Contains(cols[0].Trim('"')))
                            {
                                var entity = new Agency
                                {
                                    AgencyId = cols[0].Trim('"'),
                                    AgencyName = cols[1].Trim('"'),
                                    AgencyUrl = cols[2].Trim('"'),
                                    AgencyTimezone = cols[3].Trim('"'),
                                    AgencyLang = cols[4].Trim('"'),
                                    AgencyPhone = cols[5].Trim('"'),
                                };
                                newAgencies.Add(entity);   
                            }
                            break;
                        }

                        case "calendar.txt":
                        {
                            if (!existingCalendarIds.Contains(cols[0].Trim('"')))
                            {
                                var entity = new Data.Calendar
                                {
                                    ServiceId = cols[0].Trim('"'),
                                    Monday = cols[1] == "1",
                                    Tuesday = cols[2] == "1",
                                    Wednesday = cols[3] == "1",
                                    Thursday = cols[4] == "1",
                                    Friday = cols[5] == "1",
                                    Saturday = cols[6] == "1",
                                    Sunday = cols[7] == "1",
                                    StartDate = DateOnly.ParseExact(cols[8].Trim('"'), "yyyyMMdd", CultureInfo.InvariantCulture),
                                    EndDate = DateOnly.ParseExact(cols[9].Trim('"'), "yyyyMMdd", CultureInfo.InvariantCulture),
                                };
                                newCalendars.Add(entity);
                            }
                            break;
                        }

                        case "routes.txt":
                        {
                            if (!existingRouteIds.Contains(cols[0].Trim('"')))
                            {
                                var entity = new Data.Route
                                {
                                    RouteId = cols[0].Trim('"'),
                                    AgencyId = cols[1].Trim('"'),
                                    RouteShortName = cols[2].Trim('"'),
                                    RouteLongName = cols[3].Trim('"'),
                                    RouteDesc = cols[4].Trim('"'),
                                    RouteType = int.Parse(cols[5].Trim('"')),
                                    RouteUrl = cols[6].Trim('"'),
                                    RouteColor = cols[7].Trim('"'),
                                    RouteTextColor = cols[8].Trim('"'),
                                };
                                newRoutes.Add(entity);
                            }
                            break;
                        }

                        case "shapes.txt":
                        {
                            if (!existingShapeIds.Contains(cols[0].Trim('"') + "|" + int.Parse(cols[3].Trim('"'))))
                            {
                                if (cols[4] == "\"\"") cols[4] = "0";
                                
                                var entity = new Shape
                                {
                                    ShapeId = cols[0].Trim('"'),
                                    ShapePtLat = float.Parse(cols[1].Trim('"')),
                                    ShapePtLon = float.Parse(cols[2].Trim('"')),
                                    ShapePtSequence = int.Parse(cols[3].Trim('"')),
                                    ShapeDistTraveled = float.Parse(cols[4].Trim('"')),
                                };
                                newShapes.Add(entity);
                            }
                            break;
                        }

                        case "stops.txt":
                        {
                            if (!existingStopIds.Contains(cols[0].Trim('"')))
                            {
                                var entity = new Stop
                                {
                                    StopId = cols[0].Trim('"'),
                                    StopCode = cols[1].Trim('"'),
                                    StopName = cols[2].Trim('"'),
                                    StopDesc = cols[3].Trim('"'),
                                    StopLat = float.Parse(cols[4].Trim('"')),
                                    StopLon = float.Parse(cols[5].Trim('"')),
                                    ZoneId = cols[6].Trim('"'),
                                    StopUrl = cols[7].Trim('"'),
                                    LocationType = cols[8].Trim('"'),
                                    ParentStation = cols[9].Trim('"'),
                                    StopTimezone = cols[10].Trim('"'),
                                    WheelchairBoarding = cols[11].Trim('"'),
                                };
                                newStops.Add(entity);
                            }
                            break;
                        }

                        case "stop_times.txt":
                        {
                            if (!existingStopTimeIds.Contains(cols[0].Trim('"') + "|" + int.Parse(cols[4].Trim('"'))))
                            {
                                if (cols[8] == "\"\"") cols[8] = "0";

                                int hour1 = int.Parse(cols[1].Trim('"')[..2]);
                                int hour2 = int.Parse(cols[2].Trim('"')[..2]);
                                cols[1] = string.Concat((hour1 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[1].AsSpan(3));
                                cols[2] = string.Concat((hour2 % 24).ToString("D2", CultureInfo.InvariantCulture), cols[2].AsSpan(3));

                                var entity = new StopTime
                                {
                                    TripId = cols[0].Trim('"'),
                                    ArrivalTime = TimeOnly.ParseExact(cols[1].Trim('"'), "HH:mm:ss", CultureInfo.InvariantCulture),
                                    DepartureTime = TimeOnly.ParseExact(cols[2].Trim('"'), "HH:mm:ss", CultureInfo.InvariantCulture),
                                    StopId = cols[3].Trim('"'),
                                    StopSequence = int.Parse(cols[4].Trim('"')),
                                    StopHeadsign = cols[5].Trim('"'),
                                    PickupType = cols[6] == "1",
                                    DropOffType = cols[7] == "1",
                                    ShapeDistTraveled = float.Parse(cols[8].Trim('"')),
                                };
                                newStopTimes.Add(entity);
                            }
                            break;
                        }

                        case "trips.txt":
                        {
                            if (!existingTripIds.Contains(cols[2].Trim('"')))
                            {
                                var entity = new Trip
                                {
                                    RouteId = cols[0].Trim('"'),
                                    ServiceId = cols[1].Trim('"'),
                                    TripId = cols[2].Trim('"'),
                                    TripHeadsign = cols[3].Trim('"'),
                                    TripShortName = cols[4].Trim('"'),
                                    DirectionId = cols[5] == "1",
                                    BlockId = cols[6].Trim('"'),
                                    ShapeId = cols[7].Trim('"'),
                                    WheelchairAccessible = cols[8] == "1",
                                    VehicleCategoryId = cols[9].Trim('"'),
                                };
                                newTrips.Add(entity);
                            }
                            break;
                        }

                        case "vehicle_boardings.txt":
                        {
                            if (!existingVehicleBoardingIds.Contains(cols[0].Trim('"') + "|" + cols[1].Trim('"') + "|" + cols[2].Trim('"') + "|" + cols[3].Trim('"')))
                            {
                                var entity = new VehicleBoarding
                                {
                                    VehicleCategoryId = cols[0].Trim('"'),
                                    ChildSequence = cols[1].Trim('"'),
                                    GrandchildSequence = cols[2].Trim('"'),
                                    BoardingAreaId = cols[3].Trim('"'),
                                };
                                newVehicleBoardings.Add(entity);
                            }
                            break;
                        }

                        case "vehicle_categories.txt":
                        {
                            if (!existingVehicleCategoryIds.Contains(cols[0].Trim('"')))
                            {
                                var entity = new VehicleCategory
                                {
                                    VehicleCategoryId = cols[0].Trim('"'),
                                    VehicleCategoryName = cols[1].Trim('"'),
                                };
                                newVehicleCategories.Add(entity);
                            }
                            break;
                        }

                        case "vehicle_couplings.txt":
                        {
                            if (!existingVehicleCouplingIds.Contains(cols[0].Trim('"') + "|" + cols[1].Trim('"') + "|" + cols[2].Trim('"')))
                            {
                                var entity = new VehicleCoupling
                                {
                                    ParentId = cols[0].Trim('"'),
                                    ChildId = cols[1].Trim('"'),
                                    ChildSequence = cols[2].Trim('"'),
                                    ChildLabel = cols[3].Trim('"'),
                                };
                                newVehicleCouplings.Add(entity);
                            }
                            break;
                        }
                    }
                }
            }

            Console.WriteLine(newAgencies.Count);
            Console.WriteLine(newCalendars.Count);
            Console.WriteLine(newRoutes.Count);
            Console.WriteLine(newShapes.Count);
            Console.WriteLine(newTrips.Count);
            Console.WriteLine(newStops.Count);
            Console.WriteLine(newStopTimes.Count);
            Console.WriteLine(newVehicleBoardings.Count);
            Console.WriteLine(newVehicleCategories.Count);
            Console.WriteLine(newVehicleCouplings.Count);

            Console.WriteLine("Agencies");
            db.Agencies.AddRange(newAgencies);
            db.SaveChanges();

            Console.WriteLine("Calendars");
            db.Calendars.AddRange(newCalendars);
            db.SaveChanges();
            
            Console.WriteLine("Routes");
            db.Routes.AddRange(newRoutes);
            db.SaveChanges();
            
            Console.WriteLine("Shapes");
            db.Shapes.AddRange(newShapes);
            db.SaveChanges();
            
            Console.WriteLine("Trips");
            db.Trips.AddRange(newTrips);
            db.SaveChanges();
            
            Console.WriteLine("Stops");
            db.Stops.AddRange(newStops);
            db.SaveChanges();
            
            Console.WriteLine("StopTimes");
            int batchSize = 1000;
            for (int i = 0; i < newStopTimes.Count; i += batchSize)
            {
                var batch = newStopTimes.Skip(i).Take(batchSize).ToList();
                db.StopTimes.AddRange(batch);
                db.SaveChanges();
            }

            Console.WriteLine("VehicleCategories");
            db.VehicleCategories.AddRange(newVehicleCategories);
            db.SaveChanges();
            
            Console.WriteLine("VehicleBoardings");
            db.VehicleBoardings.AddRange(newVehicleBoardings);
            db.SaveChanges();

            Console.WriteLine("VehicleCouplings");
            db.VehicleCouplings.AddRange(newVehicleCouplings);
            db.SaveChanges();

            Console.WriteLine("Finish");
        }
    }
}