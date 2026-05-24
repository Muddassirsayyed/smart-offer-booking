using Microsoft.EntityFrameworkCore;
using SmartOfferBooking.Models;

namespace SmartOfferBooking.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferSlot> OfferSlots => Set<OfferSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e => {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Business>(e => {
            e.HasOne(b => b.User).WithOne(u => u.Business).HasForeignKey<Business>(b => b.UserId);
            e.HasIndex(b => b.UserId).IsUnique();
        });

        modelBuilder.Entity<Offer>(e => {
            e.Property(o => o.Status).HasConversion<string>();
            e.HasIndex(o => o.BusinessId);
            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.EndDate);
        });

        modelBuilder.Entity<OfferSlot>(e => {
            e.Property(s => s.Status).HasConversion<string>();
            e.HasIndex(s => s.OfferId);
            e.HasIndex(s => s.SlotDate);
        });

        modelBuilder.Entity<Booking>(e => {
            e.Property(b => b.Status).HasConversion<string>();
            e.HasIndex(b => b.BookingReference).IsUnique();
            e.HasIndex(b => b.SlotId);
            e.HasIndex(b => b.CustomerEmail);
        });

        // Seed admin user
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Name = "Admin User",
            Email = "admin@smartoffer.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Phone = "9999999999",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        modelBuilder.Entity<Business>().HasData(new Business
        {
            Id = 1,
            BusinessName = "FitZone Gym",
            BusinessType = "Gym",
            OwnerName = "Admin User",
            Phone = "9999999999",
            Email = "admin@smartoffer.com",
            Address = "123 Main Street",
            City = "Mumbai",
            OpeningTime = new TimeOnly(6, 0),
            ClosingTime = new TimeOnly(22, 0),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
}
