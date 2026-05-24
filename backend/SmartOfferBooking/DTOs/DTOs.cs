namespace SmartOfferBooking.DTOs;

// Auth DTOs
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Name, string Email, string Password, string Phone);
public record AuthResponse(string Token, string RefreshToken, UserDto User);

// User DTOs
public record UserDto(int Id, string Name, string Email, string Phone, string Role, DateTime CreatedAt);

// Business DTOs
public record BusinessDto(int Id, string BusinessName, string BusinessType, string OwnerName,
    string Phone, string Email, string Address, string City, string? LogoUrl,
    string OpeningTime, string ClosingTime, bool IsActive);

public record CreateBusinessRequest(string BusinessName, string BusinessType, string OwnerName,
    string Phone, string Email, string Address, string City, string? LogoUrl,
    string OpeningTime, string ClosingTime);

public record UpdateBusinessRequest(string BusinessName, string BusinessType, string OwnerName,
    string Phone, string Email, string Address, string City, string? LogoUrl,
    string OpeningTime, string ClosingTime);

// Offer DTOs
public record OfferDto(int Id, string Title, string Description, string Category,
    decimal OriginalPrice, decimal OfferPrice, int DiscountPercentage,
    DateTime StartDate, DateTime EndDate, string StartTime, string EndTime,
    int Capacity, int BookingLimit, string Terms, string Status, string? ImageUrl,
    int BusinessId, string BusinessName, string BusinessType, string City,
    int TotalSlots, int AvailableSlots, DateTime CreatedAt);

public record CreateOfferRequest(string Title, string Description, string Category,
    decimal OriginalPrice, decimal OfferPrice, int DiscountPercentage,
    DateTime StartDate, DateTime EndDate, string StartTime, string EndTime,
    int Capacity, int BookingLimit, string Terms, string Status, string? ImageUrl);

public record UpdateOfferRequest(string Title, string Description, string Category,
    decimal OriginalPrice, decimal OfferPrice, int DiscountPercentage,
    DateTime StartDate, DateTime EndDate, string StartTime, string EndTime,
    int Capacity, int BookingLimit, string Terms, string Status, string? ImageUrl);

// Slot DTOs
public record SlotDto(int Id, int OfferId, string OfferTitle, string SlotDate,
    string StartTime, string EndTime, int Capacity, int BookedCount,
    int AvailableCount, string Status);

public record CreateSlotRequest(string SlotDate, string StartTime, string EndTime, int Capacity);
public record UpdateSlotRequest(string SlotDate, string StartTime, string EndTime, int Capacity, string Status);

// Booking DTOs
public record BookingDto(int Id, string BookingReference, string CustomerName,
    string CustomerPhone, string CustomerEmail, int NumberOfPeople, string SpecialNote,
    string Status, string? QrCodeData, bool IsWaitlisted, int WaitlistPosition,
    int SlotId, string SlotDate, string SlotStartTime, string SlotEndTime,
    int OfferId, string OfferTitle, string BusinessName, DateTime CreatedAt);

public record CreateBookingRequest(string CustomerName, string CustomerPhone, string CustomerEmail,
    int SlotId, int NumberOfPeople, string SpecialNote);

public record UpdateBookingStatusRequest(string Status);

// Dashboard DTOs
public record DashboardSummaryDto(
    int TotalOffers, int ActiveOffers, int TotalBookings, int TodayBookings,
    int TotalCapacity, int BookedSeats, int AvailableSeats, double ConversionRate,
    List<BookingTrendDto> BookingTrend, List<CategoryStatsDto> CategoryStats,
    List<BookingDto> RecentBookings);

public record BookingTrendDto(string Date, int Count);
public record CategoryStatsDto(string Category, int Count);

// Filter DTOs
public record OfferFilterRequest(string? BusinessType, string? Category, DateTime? Date,
    decimal? MinPrice, decimal? MaxPrice, bool? AvailableOnly, string? Search,
    int Page = 1, int PageSize = 12);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
