using Microsoft.EntityFrameworkCore;

using TransportApi.Models;

namespace TransportApi.Data;

public class TransportDbContext : DbContext
{
    public TransportDbContext(DbContextOptions<TransportDbContext> options) 
        : base(options) { }

    public DbSet<Agency> Agencies { get; set; } = null!;
    public DbSet<Calendar> Calendars { get; set; } = null!;
    public DbSet<CalendarDate> CalendarDates { get; set; } = null!;
    public DbSet<Note> Notes { get; set; } = null!;
    public DbSet<Models.Route> Routes { get; set; } = null!;
    public DbSet<Trip> Trips { get; set; } = null!;
    public DbSet<Stop> Stops { get; set; } = null!;
    public DbSet<StopTime> StopTimes { get; set; } = null!;
    public DbSet<Shape> Shapes { get; set; } = null!;
    public DbSet<RealtimeStopTimeUpdate> RealtimeStopTimeUpdates { get; set; } = null!;
    public DbSet<RealtimeVehiclePosition> RealtimeVehiclePositions { get; set; } = null!;
    public DbSet<VehicleBoarding> VehicleBoardings { get; set; } = null!;
    public DbSet<VehicleCategory> VehicleCategories { get; set; } = null!;
    public DbSet<VehicleCoupling> VehicleCouplings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StopTime>()
            .HasKey(st => new { st.TripId, st.StopSequence });

        modelBuilder.Entity<RealtimeStopTimeUpdate>()
            .HasKey(rt => new { rt.TripId, rt.StopSequence });

        modelBuilder.Entity<RealtimeVehiclePosition>()
            .HasKey(rv => rv.VehicleId);

        modelBuilder.Entity<Shape>()
            .HasKey(s => new { s.Id, s.Sequence });

        modelBuilder.Entity<VehicleBoarding>()
            .HasKey(vb => new { vb.VehicleCategoryId, vb.ChildSequence, vb.GrandchildSequence, vb.BoardingAreaId });
            
        modelBuilder.Entity<VehicleCoupling>()
            .HasKey(vc => new { vc.ParentId, vc.ChildId, vc.ChildSequence });
    }
}
