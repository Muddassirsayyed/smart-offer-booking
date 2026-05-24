using Microsoft.EntityFrameworkCore;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Models;

namespace SmartOfferBooking.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IBookingService _bookingService;

    public DashboardService(AppDbContext db, IBookingService bookingService)
    {
        _db = db;
        _bookingService = bookingService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int businessId)
    {
        var offers = await _db.Offers.Where(o => o.BusinessId == businessId).ToListAsync();
        var slots = await _db.OfferSlots.Include(s => s.Offer)
            .Where(s => s.Offer.BusinessId == businessId).ToListAsync();
        var bookings = await _db.Bookings.Include(b => b.Slot).ThenInclude(s => s.Offer)
            .ThenInclude(o => o.Business)
            .Where(b => b.Slot.Offer.BusinessId == businessId).ToListAsync();

        var today = DateTime.UtcNow.Date;
        var totalCapacity = slots.Sum(s => s.Capacity);
        var bookedSeats = slots.Sum(s => s.BookedCount);
        var activeOffers = offers.Count(o => o.Status == OfferStatus.Active);
        var totalBookings = bookings.Count(b => b.Status != BookingStatus.Cancelled);
        var todayBookings = bookings.Count(b => b.CreatedAt.Date == today);
        var conversionRate = totalCapacity > 0 ? (double)bookedSeats / totalCapacity * 100 : 0;

        var trend = bookings
            .Where(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(b => b.CreatedAt.Date.ToString("MMM dd"))
            .Select(g => new BookingTrendDto(g.Key, g.Count()))
            .OrderBy(x => x.Date).ToList();

        var categoryStats = offers
            .GroupBy(o => o.Category)
            .Select(g => new CategoryStatsDto(g.Key, g.Count()))
            .ToList();

        var recentBookings = bookings.OrderByDescending(b => b.CreatedAt).Take(10)
            .Select(b => new BookingDto(b.Id, b.BookingReference, b.CustomerName,
                b.CustomerPhone, b.CustomerEmail, b.NumberOfPeople, b.SpecialNote,
                b.Status.ToString(), null, b.IsWaitlisted, b.WaitlistPosition,
                b.Slot.Id, b.Slot.SlotDate.ToString("yyyy-MM-dd"),
                b.Slot.StartTime.ToString("HH:mm"), b.Slot.EndTime.ToString("HH:mm"),
                b.Slot.Offer?.Id ?? 0, b.Slot.Offer?.Title ?? "",
                b.Slot.Offer?.Business?.BusinessName ?? "", b.CreatedAt))
            .ToList();

        return new DashboardSummaryDto(offers.Count, activeOffers, totalBookings, todayBookings,
            totalCapacity, bookedSeats, totalCapacity - bookedSeats,
            Math.Round(conversionRate, 1), trend, categoryStats, recentBookings);
    }
}
