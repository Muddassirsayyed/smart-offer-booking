using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartOfferBooking.Models;

public class User
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(20)] public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Business? Business { get; set; }
}

public enum UserRole { Admin, Customer }

public class Business
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string BusinessName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string BusinessType { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string OwnerName { get; set; } = string.Empty;
    [MaxLength(20)] public string Phone { get; set; } = string.Empty;
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(500)] public string Address { get; set; } = string.Empty;
    [MaxLength(100)] public string City { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("User")] public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}

public class Offer
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)] public string Description { get; set; } = string.Empty;
    [MaxLength(100)] public string Category { get; set; } = string.Empty;
    [Column(TypeName = "decimal(10,2)")] public decimal OriginalPrice { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal OfferPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Capacity { get; set; }
    public int BookingLimit { get; set; } = 1;
    [MaxLength(2000)] public string Terms { get; set; } = string.Empty;
    public OfferStatus Status { get; set; } = OfferStatus.Draft;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("Business")] public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public ICollection<OfferSlot> Slots { get; set; } = new List<OfferSlot>();
}

public enum OfferStatus { Draft, Active, Paused, Expired, Cancelled }

public class OfferSlot
{
    [Key] public int Id { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; } = 0;
    public SlotStatus Status { get; set; } = SlotStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("Offer")] public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    [NotMapped] public int AvailableCount => Capacity - BookedCount;
}

public enum SlotStatus { Active, Full, Cancelled, Completed }

public class Booking
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(20)] public string BookingReference { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string CustomerName { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string CustomerPhone { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string CustomerEmail { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; } = 1;
    [MaxLength(500)] public string SpecialNote { get; set; } = string.Empty;
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? QrCodeData { get; set; }
    public bool IsWaitlisted { get; set; } = false;
    public int WaitlistPosition { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("OfferSlot")] public int SlotId { get; set; }
    public OfferSlot Slot { get; set; } = null!;
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}

public enum BookingStatus { Pending, Confirmed, Cancelled, Completed, NoShow }

public class NotificationLog
{
    [Key] public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Sent";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("Booking")] public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
}
