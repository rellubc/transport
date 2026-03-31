using Microsoft.EntityFrameworkCore;
using TransportStatic.Data;
using TransportStatic.Models;
using TransportStatic.Services;

public class RealtimePollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RealtimePollingService> _logger;

    public RealtimePollingService(IServiceProvider serviceProvider, ILogger<RealtimePollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Realtime polling service started...");
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));

        try
        {
            await PollRealtimeData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during realtime polling");
        }

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PollRealtimeData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during realtime polling");
            }
        }

        _logger.LogInformation("Realtime polling service stopped");
    }

    private async Task PollRealtimeData()
    {
        using var scope = _serviceProvider.CreateScope();
        var realtimeService = scope.ServiceProvider.GetRequiredService<IRealtimeService>();
        var db = scope.ServiceProvider.GetRequiredService<TransportDbContext>();

        var metroVehicles = await realtimeService.GetRealtimeVehicles("Metro");
        _logger.LogInformation("Fetching realtime metro vehicles...");

        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        foreach (var vehicle in metroVehicles)
        {
            
        }

        _logger.LogInformation("Fetching realtime metro trip updates...");

        // position in consist reverse for metro when directionId = 0
        var tripUpdates = await realtimeService.GetRealtimeTripUpdates("Metro");
        var vehiclePositions = await realtimeService.GetRealtimeVehiclePositions("Metro");
        var newTripUpdates = new List<TripUpdate>();
        var newVehiclePositions = new List<VehiclePosition>();

        foreach (var tripUpdate in tripUpdates)
        {
            newTripUpdates.AddRange(TripUpdate.Parse(tripUpdate, "Metro"));
        }

        foreach (var vehiclePosition in vehiclePositions)
        {
            newVehiclePositions.AddRange(VehiclePosition.Parse(vehiclePosition, "Metro"));
        }

        foreach (var tripUpdate in newTripUpdates)
        {
            var existing = await db.TripUpdates.FindAsync(tripUpdate.TripId, tripUpdate.StuStopSequence, tripUpdate.StuCarriagePositionInConsist);

            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(tripUpdate);
            }
            else
            {
                db.TripUpdates.Add(tripUpdate);
            }
        }
        
        await db.SaveChangesAsync();
        
        foreach (var vehiclePosition in newVehiclePositions)
        {
            var existing = await db.VehiclePositions.FindAsync(vehiclePosition.VehicleId, vehiclePosition.StuCarriagePositionInConsist);

            if (existing != null)
            {
                db.Entry(existing).CurrentValues.SetValues(vehiclePosition);
            }
            else
            {
                db.VehiclePositions.Add(vehiclePosition);
            }
        }

        await db.SaveChangesAsync();

        // var railVehicles = await realtimeService.GetRealtimeVehicles("Rail");
        // _logger.LogInformation("Fetching realtime rail vehicles...");

        // foreach (var vehicle in railVehicles)
        // {
        //     if (vehicle.Trip == null || vehicle.Trip.TripId == null) continue;
        //     _logger.LogInformation("Fetching realtime trip update...");

        //     var tripUpdate = await realtimeService.GetRealtimeTripUpdate(vehicle.Trip.TripId, "Rail");
        // }
    }
}