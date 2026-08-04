using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Models;

namespace SparkyProject.API.Data;

// Owner: Team Lead (Sprint 1 setup) — everyone registers their own DbSets here after ERD is finalized.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Murooj Al Shehaibi
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Hotel> Hotels { get; set; } = null!;

    // Khalid Al Hashemi
    public DbSet<RoomType> RoomTypes { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;

    // Ibrahim Al Kindi
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    // Ruqaya
    public DbSet<GuestProfile> GuestProfiles { get; set; } = null!;
    public DbSet<Amenity> Amenities { get; set; } = null!;

    // Aisha Mubarak ALHashmi
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;

    // Ahmed Almalki
    public DbSet<Staff> Staff { get; set; } = null!;
    public DbSet<Promotion> Promotions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // TODO: configure relationships, composite keys, and constraints once the ERD is finalized.
    }
}
