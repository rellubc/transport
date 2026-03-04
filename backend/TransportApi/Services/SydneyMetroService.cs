using Microsoft.EntityFrameworkCore;

using TransportApi.Data;
using TransportApi.Models;

using Google.Protobuf;
using System.Xml.Serialization;
using System.Text.Json;

using TransitRealtime;

namespace TransportApi.Services;

public interface ISydneyMetroService
{
    Task<List<TripUpdate>> SydneyMetroTripUpdates(string TripId);
    Task<List<VehiclePosition>> SydneyMetroVehiclePositions();
}

public class SydneyMetroService(TransportDbContext db, IHttpClientFactory factory, ILogger<SydneyMetroService> logger) : ISydneyMetroService
{
    private readonly IHttpClientFactory _factory = factory;
    private readonly TransportDbContext _db = db;
    private readonly ILogger<SydneyMetroService> _logger = logger;

    public async Task<List<TripUpdate>> SydneyMetroTripUpdates(string tripId)
    {
        _logger.LogInformation("Updating vehicle trip details...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v2/gtfs/realtime/metro");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return [];
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var feed = TransitRealtime.FeedMessage.Parser.ParseFrom(responseStream);

        var newTripUpdates = new List<TripUpdate>();

        if (feed == null) return [];

        foreach (var entity in feed.Entity)
        {
            try
            {
                if (entity.TripUpdate == null) continue;
                if (entity.TripUpdate.Trip.TripId != tripId) continue;
                newTripUpdates.Add(entity.TripUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process realtime entity {EntityId}", entity.Id);
            }
        }

        return newTripUpdates;
    }

    public async Task<List<VehiclePosition>> SydneyMetroVehiclePositions()
    {
        _logger.LogInformation("Updating vehicle positions...");
        var client = _factory.CreateClient("TransportNSW");
        var response = await client.GetAsync("https://api.transport.nsw.gov.au/v2/gtfs/vehiclepos/metro");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch data: {Response}", response.StatusCode);
            return [];
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var feed = TransitRealtime.FeedMessage.Parser.ParseFrom(responseStream);

        var newVehiclePositions = new List<VehiclePosition>();

        if (feed == null) return [];

        foreach (var entity in feed.Entity)
        {
            try
            {
                if (entity.Vehicle == null) continue;
                newVehiclePositions.Add(entity.Vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process vehicle entity {EntityId}", entity.Id);
            }
        }
        return newVehiclePositions;
    }
}
