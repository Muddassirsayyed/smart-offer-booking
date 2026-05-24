using Microsoft.EntityFrameworkCore;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Models;

namespace SmartOfferBooking.Services;

public class OfferService : IOfferService
{
    private readonly AppDbContext _db;
    public OfferService(AppDbContext db) => _db = db;

    public async Task<PagedResult<OfferDto>> GetPublicOffersAsync(OfferFilterRequest f)
    {
        var query = _db.Offers
            .Include(o => o.Business)
            .Include(o => o.Slots)
            .Where(o => o.Status == OfferStatus.Active && o.EndDate >= DateTime.UtcNow)
            .AsQueryable();

        if (!string.IsNullOrEmpty(f.BusinessType))
            query = query.Where(o => o.Business.BusinessType == f.BusinessType);

        if (!string.IsNullOrEmpty(f.Category))
            query = query.Where(o => o.Category == f.Category);

        if (f.Date.HasValue)
            query = query.Where(o =>
                o.Slots.Any(s => s.SlotDate == DateOnly.FromDateTime(f.Date.Value)));

        if (f.MinPrice.HasValue)
            query = query.Where(o => o.OfferPrice >= f.MinPrice.Value);

        if (f.MaxPrice.HasValue)
            query = query.Where(o => o.OfferPrice <= f.MaxPrice.Value);

        if (f.AvailableOnly == true)
            query = query.Where(o =>
                o.Slots.Any(s =>
                    s.Status == SlotStatus.Active &&
                    s.BookedCount < s.Capacity));

        if (!string.IsNullOrEmpty(f.Search))
            query = query.Where(o =>
                o.Title.Contains(f.Search) ||
                o.Business.BusinessName.Contains(f.Search));

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .ToListAsync();

        return new PagedResult<OfferDto>(
            items.Select(Map).ToList(),
            total,
            f.Page,
            f.PageSize,
            (int)Math.Ceiling((double)total / f.PageSize)
        );
    }

    public async Task<OfferDto?> GetByIdAsync(int id)
    {
        var o = await _db.Offers
            .Include(x => x.Business)
            .Include(x => x.Slots)
            .FirstOrDefaultAsync(x => x.Id == id);

        return o == null ? null : Map(o);
    }

    public async Task<List<OfferDto>> GetByBusinessAsync(int businessId)
    {
        var offers = await _db.Offers
            .Include(o => o.Business)
            .Include(o => o.Slots)
            .Where(o => o.BusinessId == businessId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return offers.Select(Map).ToList();
    }

    public async Task<OfferDto> CreateAsync(int businessId, CreateOfferRequest r)
    {
        var o = new Offer
        {
            Title = r.Title,
            Description = r.Description,
            Category = r.Category,

            OriginalPrice = r.OriginalPrice,
            OfferPrice = r.OfferPrice,
            DiscountPercentage = r.DiscountPercentage,

            // FIXED UTC ISSUE
            StartDate = DateTime.SpecifyKind(r.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(r.EndDate, DateTimeKind.Utc),

            StartTime = TimeOnly.Parse(r.StartTime),
            EndTime = TimeOnly.Parse(r.EndTime),

            Capacity = r.Capacity,
            BookingLimit = r.BookingLimit,

            Terms = r.Terms,

            Status = Enum.Parse<OfferStatus>(r.Status),

            ImageUrl = r.ImageUrl,

            BusinessId = businessId
        };

        _db.Offers.Add(o);

        await _db.SaveChangesAsync();

        await _db.Entry(o)
            .Reference(x => x.Business)
            .LoadAsync();

        return Map(o);
    }

    public async Task<OfferDto?> UpdateAsync(
        int id,
        int businessId,
        UpdateOfferRequest r)
    {
        var o = await _db.Offers
            .Include(x => x.Business)
            .Include(x => x.Slots)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == businessId);

        if (o == null)
            return null;

        o.Title = r.Title;
        o.Description = r.Description;
        o.Category = r.Category;

        o.OriginalPrice = r.OriginalPrice;
        o.OfferPrice = r.OfferPrice;
        o.DiscountPercentage = r.DiscountPercentage;

        // FIXED UTC ISSUE
        o.StartDate = DateTime.SpecifyKind(r.StartDate, DateTimeKind.Utc);
        o.EndDate = DateTime.SpecifyKind(r.EndDate, DateTimeKind.Utc);

        o.StartTime = TimeOnly.Parse(r.StartTime);
        o.EndTime = TimeOnly.Parse(r.EndTime);

        o.Capacity = r.Capacity;
        o.BookingLimit = r.BookingLimit;

        o.Terms = r.Terms;

        o.Status = Enum.Parse<OfferStatus>(r.Status);

        o.ImageUrl = r.ImageUrl;

        o.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Map(o);
    }

    public async Task<bool> DeleteAsync(int id, int businessId)
    {
        var o = await _db.Offers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.BusinessId == businessId);

        if (o == null)
            return false;

        _db.Offers.Remove(o);

        await _db.SaveChangesAsync();

        return true;
    }

    private static OfferDto Map(Offer o)
    {
        var activeSlots = o.Slots
            .Where(s => s.Status == SlotStatus.Active)
            .ToList();

        return new OfferDto(
            o.Id,
            o.Title,
            o.Description,
            o.Category,
            o.OriginalPrice,
            o.OfferPrice,
            o.DiscountPercentage,
            o.StartDate,
            o.EndDate,
            o.StartTime.ToString("HH:mm"),
            o.EndTime.ToString("HH:mm"),
            o.Capacity,
            o.BookingLimit,
            o.Terms,
            o.Status.ToString(),
            o.ImageUrl,
            o.BusinessId,
            o.Business?.BusinessName ?? "",
            o.Business?.BusinessType ?? "",
            o.Business?.City ?? "",
            o.Slots.Count,
            activeSlots.Sum(s => s.Capacity - s.BookedCount),
            o.CreatedAt
        );
    }
}