using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace backend.Data
{
    public class TransportDbContext : DbContext
    {
        public TransportDbContext(DbContextOptions<TransportDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Agency> Agencies { get; set; } = null!;
        public DbSet<Calendar> Calendars { get; set; } = null!;
        public DbSet<Route> Routes { get; set; } = null!;
        public DbSet<Trip> Trips { get; set; } = null!;
        public DbSet<Stop> Stops { get; set; } = null!;
        public DbSet<StopTime> StopTimes { get; set; } = null!;
        public DbSet<Shape> Shapes { get; set; } = null!;
        public DbSet<VehicleBoarding> VehicleBoardings { get; set; } = null!;
        public DbSet<VehicleCategory> VehicleCategories { get; set; } = null!;
        public DbSet<VehicleCoupling> VehicleCouplings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agency>()
                .HasKey(a => a.AgencyId);

            modelBuilder.Entity<Calendar>()
                .HasKey(c => c.ServiceId);

            modelBuilder.Entity<Route>()
                .HasKey(r => r.RouteId);

            modelBuilder.Entity<Trip>()
                .HasKey(t => t.TripId);

            modelBuilder.Entity<Stop>()
                .HasKey(s => s.StopId);

            modelBuilder.Entity<StopTime>()
                .HasKey(st => new { st.TripId, st.StopSequence });

            modelBuilder.Entity<Shape>()
                .HasKey(s => new { s.ShapeId, s.ShapePtSequence });

            modelBuilder.Entity<VehicleBoarding>()
                .HasKey(vb => new { vb.VehicleCategoryId, vb.ChildSequence, vb.GrandchildSequence, vb.BoardingAreaId });

            modelBuilder.Entity<VehicleCategory>()
                .HasKey(vc => vc.VehicleCategoryId);
                
            modelBuilder.Entity<VehicleCoupling>()
                .HasKey(vc => new { vc.ParentId, vc.ChildId, vc.ChildSequence });
        }
    }
}
