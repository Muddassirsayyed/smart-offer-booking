using Microsoft.EntityFrameworkCore;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Models;

namespace SmartOfferBooking.Services;

public class BusinessService : IBusinessService
{
    private readonly AppDbContext _db;
    public BusinessService(AppDbContext db) => _db = db;

    public async Task<BusinessDto?> GetByUserIdAsync(int userId)
    {
        var b = await _db.Businesses.FirstOrDefaultAsync(x => x.UserId == userId);
        return b == null ? null : Map(b);
    }

    public async Task<BusinessDto> CreateAsync(int userId, CreateBusinessRequest r)
    {
        var b = new Business
        {
            BusinessName = r.BusinessName, BusinessType = r.BusinessType,
            OwnerName = r.OwnerName, Phone = r.Phone, Email = r.Email,
            Address = r.Address, City = r.City, LogoUrl = r.LogoUrl,
            OpeningTime = TimeOnly.Parse(r.OpeningTime),
            ClosingTime = TimeOnly.Parse(r.ClosingTime),
            UserId = userId
        };
        _db.Businesses.Add(b);
        await _db.SaveChangesAsync();
        return Map(b);
    }

    public async Task<BusinessDto?> UpdateAsync(int userId, UpdateBusinessRequest r)
    {
        var b = await _db.Businesses.FirstOrDefaultAsync(x => x.UserId == userId);
        if (b == null) return null;
        b.BusinessName = r.BusinessName; b.BusinessType = r.BusinessType;
        b.OwnerName = r.OwnerName; b.Phone = r.Phone; b.Email = r.Email;
        b.Address = r.Address; b.City = r.City; b.LogoUrl = r.LogoUrl;
        b.OpeningTime = TimeOnly.Parse(r.OpeningTime);
        b.ClosingTime = TimeOnly.Parse(r.ClosingTime);
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(b);
    }

    private static BusinessDto Map(Business b) => new(b.Id, b.BusinessName, b.BusinessType,
        b.OwnerName, b.Phone, b.Email, b.Address, b.City, b.LogoUrl,
        b.OpeningTime.ToString("HH:mm"), b.ClosingTime.ToString("HH:mm"), b.IsActive);
}
