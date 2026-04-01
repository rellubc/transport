using Microsoft.EntityFrameworkCore;
using TransportStatic.Models;

namespace TransportStatic.Data;

public class TransportDbContext : DbContext
{
    public TransportDbContext(DbContextOptions<TransportDbContext> options) 
        : base(options)
    {}

    public DbSet<Agency> Agencies { get; set; } = null!;
    public DbSet<Calendar> Calendars { get; set; } = null!;
    public DbSet<Occupancy> Occupancies { get; set; } = null!;
    public DbSet<Note> Notes { get; set; } = null!;
    public DbSet<Models.Route> Routes { get; set; } = null!;
    public DbSet<Trip> Trips { get; set; } = null!;
    public DbSet<Stop> Stops { get; set; } = null!;
    public DbSet<StopTime> StopTimes { get; set; } = null!;
    public DbSet<Shape> Shapes { get; set; } = null!;
    public DbSet<VehicleBoarding> VehicleBoardings { get; set; } = null!;
    public DbSet<VehicleCategory> VehicleCategories { get; set; } = null!;
    public DbSet<VehicleCoupling> VehicleCouplings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Occupancy>()
            .HasKey(o => new { o.TripId, o.StopSequence, o.StartDate });
            
        modelBuilder.Entity<Stop>()
            .HasKey(s => new { s.Id, s.Mode });

        modelBuilder.Entity<StopTime>()
            .HasKey(st => new { st.TripId, st.StopSequence });

        modelBuilder.Entity<Shape>()
            .HasKey(s => new { s.Id, s.Sequence });

        modelBuilder.Entity<VehicleBoarding>()
            .HasKey(vb => new { vb.VehicleCategoryId, vb.ChildSequence, vb.BoardingAreaId });
            
        modelBuilder.Entity<VehicleCoupling>()
            .HasKey(vc => new { vc.ParentId, vc.ChildId, vc.ChildSequence });
    }
}
