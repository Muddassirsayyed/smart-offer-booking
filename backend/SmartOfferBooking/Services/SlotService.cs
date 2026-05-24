using Microsoft.EntityFrameworkCore;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Models;

namespace SmartOfferBooking.Services;

public class SlotService : ISlotService
{
    private readonly AppDbContext _db;
    public SlotService(AppDbContext db) => _db = db;

    public async Task<List<SlotDto>> GetByOfferAsync(int offerId)
    {
        var slots = await _db.OfferSlots.Include(s => s.Offer)
            .Where(s => s.OfferId == offerId).OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime).ToListAsync();
        return slots.Select(Map).ToList();
    }

    public async Task<SlotDto?> GetByIdAsync(int id)
    {
        var s = await _db.OfferSlots.Include(x => x.Offer).FirstOrDefaultAsync(x => x.Id == id);
        return s == null ? null : Map(s);
    }

    public async Task<SlotDto> CreateAsync(int offerId, CreateSlotRequest r)
    {
        var slot = new OfferSlot
        {
            OfferId = offerId,
            SlotDate = DateOnly.Parse(r.SlotDate),
            StartTime = TimeOnly.Parse(r.StartTime),
            EndTime = TimeOnly.Parse(r.EndTime),
            Capacity = r.Capacity
        };
        _db.OfferSlots.Add(slot);
        await _db.SaveChangesAsync();
        await _db.Entry(slot).Reference(x => x.Offer).LoadAsync();
        return Map(slot);
    }

    public async Task<SlotDto?> UpdateAsync(int id, UpdateSlotRequest r)
    {
        var slot = await _db.OfferSlots.Include(x => x.Offer).FirstOrDefaultAsync(x => x.Id == id);
        if (slot == null) return null;
        slot.SlotDate = DateOnly.Parse(r.SlotDate);
        slot.StartTime = TimeOnly.Parse(r.StartTime);
        slot.EndTime = TimeOnly.Parse(r.EndTime);
        slot.Capacity = r.Capacity;
        slot.Status = Enum.Parse<SlotStatus>(r.Status);
        await _db.SaveChangesAsync();
        return Map(slot);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var slot = await _db.OfferSlots.FindAsync(id);
        if (slot == null) return false;
        _db.OfferSlots.Remove(slot);
        await _db.SaveChangesAsync();
        return true;
    }

    private static SlotDto Map(OfferSlot s) => new(s.Id, s.OfferId,
        s.Offer?.Title ?? "", s.SlotDate.ToString("yyyy-MM-dd"),
        s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
        s.Capacity, s.BookedCount, s.Capacity - s.BookedCount, s.Status.ToString());
}
