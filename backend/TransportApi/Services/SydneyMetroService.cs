using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.Models;

using Google.Protobuf;
using TransitRealtime;
using System.Formats.Tar;

namespace TransportApi.Services;

public interface ISydneyMetroService
{
    Task RealtimeSydneyMetroTripUpdate();
    Task RealtimeSydneyMetroVehiclePos();
}

public class SydneyMetroService : ISydneyMetroService
{
    private readonly IHttpClientFactory _factory;
    private readonly TransportDbContext _db;
    private readonly ILogger<SydneyMetroService> _logger;

    public SydneyMetroService(TransportDbContext db, IHttpClientFactory factory, ILogger<SydneyMetroService> logger)
    {
        _factory = factory;
        _db = db;
        _logger = logger;
    }

    public async Task RealtimeSydneyMetroTripUpdate()
    {
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v2/gtfs/realtime/metro");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Failed to fetch data: {response.StatusCode}");
            return;
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var feed = FeedMessage.Parser.ParseFrom(responseStream);

        var existingRealtimeStopTimeIds = new HashSet<string>(_db.RealtimeStopTimeUpdates.Select(rtsu => rtsu.EntityId + "|" + rtsu.TripId + "|" + rtsu.StopSequence));

        var newRealtime = new List<RealtimeStopTimeUpdate>();

        foreach (var entity in feed.Entity)
        {
            try
            {
                if (entity.TripUpdate == null) continue;

                var tripUpdate = entity.TripUpdate;
                var tripId = tripUpdate.Trip != null && tripUpdate.Trip.HasTripId ? tripUpdate.Trip.TripId : null;

                foreach (var stu in tripUpdate.StopTimeUpdate)
                {
                    int? stopSequence = stu.HasStopSequence ? (int)stu.StopSequence : (int?)null;

                    int? stopId = null;
                    if (stu.HasStopId && !string.IsNullOrEmpty(stu.StopId))
                    {
                        if (int.TryParse(stu.StopId, out var parsed)) stopId = parsed;
                    }

                    DateTime? arrival = null;
                    try { if (stu.Arrival != null && stu.Arrival.HasTime) arrival = DateTimeOffset.FromUnixTimeSeconds(stu.Arrival.Time).UtcDateTime; } catch { }

                    DateTime? departure = null;
                    try { if (stu.Departure != null && stu.Departure.HasTime) departure = DateTimeOffset.FromUnixTimeSeconds(stu.Departure.Time).UtcDateTime; } catch { }

                    if (!existingRealtimeStopTimeIds.Contains(entity.Id + "|" + tripId + "|" + stopSequence))
                    {
                        var entityRow = new RealtimeStopTimeUpdate
                        {
                            EntityId = entity.Id ?? Guid.NewGuid().ToString(),
                            TripId = tripId ?? string.Empty,
                            StopSequence = stopSequence,
                            StopId = stopId,
                            ArrivalTime = arrival,
                            DepartureTime = departure,
                            ScheduleRelationship = stu.HasScheduleRelationship ? stu.ScheduleRelationship.ToString() : null,
                            InsertedAt = DateTime.UtcNow,
                        };

                        newRealtime.Add(entityRow);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process realtime entity {EntityId}", entity.Id);
            }
        }

        if (newRealtime.Count > 0)
        {
            _logger.LogInformation("Saving {Count} realtime stop time updates", newRealtime.Count);
            _db.RealtimeStopTimeUpdates.AddRange(newRealtime);
            await _db.SaveChangesAsync();
        }
    }

    public async Task RealtimeSydneyMetroVehiclePos()
    {
        _logger.LogInformation("Updating vehicle positions...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v2/gtfs/vehiclepos/metro");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Failed to fetch data: {response.StatusCode}");
            return;
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var feed = FeedMessage.Parser.ParseFrom(responseStream);

        var newVehiclePositions = new List<RealtimeVehiclePosition>();

        foreach (var entity in feed.Entity)
        {
            try
            {
                if (entity.Vehicle == null) continue;

                var vp = entity.Vehicle;

                var row = new RealtimeVehiclePosition
                {
                    EntityId = entity.Id ?? Guid.NewGuid().ToString(),
                    VehicleId = vp.Vehicle.Id,
                    Label = vp.Vehicle.Label,
                    LicensePlate = vp.Vehicle.LicensePlate,
                    Latitude = (decimal)vp.Position.Latitude,
                    Longitude = (decimal)vp.Position.Longitude,
                    Bearing = (decimal)vp.Position.Bearing,
                    Speed = (decimal)vp.Position.Speed,
                    TripId = vp.Trip.TripId,
                    CurrentStopSequence = vp.CurrentStopSequence,
                    StopId = int.Parse(vp.StopId),
                    CurrentStatus = vp.HasCurrentStatus ? vp.CurrentStatus.ToString() : null,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)vp.Timestamp).UtcDateTime,
                    CongestionLevel = vp.HasCongestionLevel ? vp.CongestionLevel.ToString() : null,
                    OccupancyStatus = vp.HasOccupancyStatus ? vp.OccupancyStatus.ToString() : null,
                    InsertedAt = DateTime.UtcNow,
                };

                newVehiclePositions.Add(row);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process vehicle entity {EntityId}", entity.Id);
            }
        }

        var vehicles = await _db.RealtimeVehiclePositions.OrderBy(rvp => rvp.VehicleId).ToListAsync();
        
        if (newVehiclePositions.Count > 0)
        {
            foreach (var vp in newVehiclePositions)
            {
                var existing = _db.RealtimeVehiclePositions.FirstOrDefault(rvp => rvp.VehicleId == vp.VehicleId);
                if (existing == null) {
                    _db.RealtimeVehiclePositions.Add(vp);
                } else {
                    _db.Entry(existing).CurrentValues.SetValues(vp);
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
