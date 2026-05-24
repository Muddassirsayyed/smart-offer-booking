using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Models;

namespace SmartOfferBooking.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    public BookingService(AppDbContext db) => _db = db;

    public async Task<BookingDto> CreateAsync(CreateBookingRequest r)
    {
        var slot = await _db.OfferSlots.Include(s => s.Offer).ThenInclude(o => o.Business)
            .FirstOrDefaultAsync(s => s.Id == r.SlotId)
            ?? throw new InvalidOperationException("Slot not found");

        if (slot.Status != SlotStatus.Active)
            throw new InvalidOperationException("Slot is not active");
        if (slot.Offer.Status != OfferStatus.Active)
            throw new InvalidOperationException("Offer is not active");
        if (slot.Offer.EndDate < DateTime.UtcNow)
            throw new InvalidOperationException("Offer has expired");

        var existingBookings = await _db.Bookings
            .Where(b => b.SlotId == r.SlotId && b.CustomerEmail == r.CustomerEmail
                && b.Status != BookingStatus.Cancelled)
            .SumAsync(b => b.NumberOfPeople);

        if (existingBookings + r.NumberOfPeople > slot.Offer.BookingLimit)
            throw new InvalidOperationException($"Booking limit of {slot.Offer.BookingLimit} per customer exceeded");

        bool isWaitlisted = slot.BookedCount + r.NumberOfPeople > slot.Capacity;
        int waitlistPos = 0;

        if (isWaitlisted)
        {
            waitlistPos = await _db.Bookings.CountAsync(b => b.SlotId == r.SlotId && b.IsWaitlisted) + 1;
        }
        else
        {
            slot.BookedCount += r.NumberOfPeople;
            if (slot.BookedCount >= slot.Capacity) slot.Status = SlotStatus.Full;
        }

        var reference = GenerateReference();
        var qrData = GenerateQrCode(reference);

        var booking = new Booking
        {
            BookingReference = reference,
            CustomerName = r.CustomerName,
            CustomerPhone = r.CustomerPhone,
            CustomerEmail = r.CustomerEmail,
            NumberOfPeople = r.NumberOfPeople,
            SpecialNote = r.SpecialNote,
            Status = isWaitlisted ? BookingStatus.Pending : BookingStatus.Confirmed,
            QrCodeData = qrData,
            IsWaitlisted = isWaitlisted,
            WaitlistPosition = waitlistPos,
            SlotId = r.SlotId
        };

        _db.Bookings.Add(booking);

        // Mock notification log
        _db.NotificationLogs.Add(new NotificationLog
        {
            Type = "Email",
            Recipient = r.CustomerEmail,
            Message = $"Booking {reference} confirmed for {slot.Offer.Title}",
            Status = "Sent",
            BookingId = booking.Id
        });

        await _db.SaveChangesAsync();
        return MapBooking(booking, slot);
    }

    public async Task<BookingDto?> GetByReferenceAsync(string reference)
    {
        var b = await _db.Bookings.Include(x => x.Slot).ThenInclude(s => s.Offer)
            .ThenInclude(o => o.Business).FirstOrDefaultAsync(x => x.BookingReference == reference);
        return b == null ? null : MapBooking(b, b.Slot);
    }

    public async Task<List<BookingDto>> GetByBusinessAsync(int businessId, string? status)
    {
        var query = _db.Bookings.Include(b => b.Slot).ThenInclude(s => s.Offer)
            .ThenInclude(o => o.Business)
            .Where(b => b.Slot.Offer.BusinessId == businessId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, out var s))
            query = query.Where(b => b.Status == s);

        var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return bookings.Select(b => MapBooking(b, b.Slot)).ToList();
    }

    public async Task<BookingDto?> UpdateStatusAsync(int id, string status)
    {
        var b = await _db.Bookings.Include(x => x.Slot).ThenInclude(s => s.Offer)
            .ThenInclude(o => o.Business).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return null;

        var newStatus = Enum.Parse<BookingStatus>(status);
        if (newStatus == BookingStatus.Cancelled && b.Status != BookingStatus.Cancelled)
        {
            b.Slot.BookedCount = Math.Max(0, b.Slot.BookedCount - b.NumberOfPeople);
            if (b.Slot.Status == SlotStatus.Full) b.Slot.Status = SlotStatus.Active;
        }
        b.Status = newStatus;
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapBooking(b, b.Slot);
    }

    public async Task<byte[]> ExportCsvAsync(int businessId)
    {
        var bookings = await GetByBusinessAsync(businessId, null);
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(bookings);
        await writer.FlushAsync();
        return ms.ToArray();
    }

    private static string GenerateReference() =>
        "SOB" + DateTime.UtcNow.ToString("yyyyMMdd") + Random.Shared.Next(1000, 9999);

    private static string GenerateQrCode(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return Convert.ToBase64String(qrCode.GetGraphic(20));
    }

    private static BookingDto MapBooking(Booking b, OfferSlot slot) => new(
        b.Id, b.BookingReference, b.CustomerName, b.CustomerPhone, b.CustomerEmail,
        b.NumberOfPeople, b.SpecialNote, b.Status.ToString(), b.QrCodeData,
        b.IsWaitlisted, b.WaitlistPosition, slot.Id,
        slot.SlotDate.ToString("yyyy-MM-dd"), slot.StartTime.ToString("HH:mm"),
        slot.EndTime.ToString("HH:mm"), slot.Offer?.Id ?? 0,
        slot.Offer?.Title ?? "", slot.Offer?.Business?.BusinessName ?? "", b.CreatedAt);
}
