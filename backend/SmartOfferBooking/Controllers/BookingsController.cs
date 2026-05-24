using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Services;

namespace SmartOfferBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    private readonly AppDbContext _db;

    public BookingsController(IBookingService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetByReference), new { reference = result.BookingReference }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("reference/{reference}")]
    public async Task<IActionResult> GetByReference(string reference)
    {
        var result = await _service.GetByReferenceAsync(reference);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("my"), Authorize]
    public async Task<IActionResult> GetMyBookings([FromQuery] string? status)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.UserId == GetUserId());
        if (business == null) return NotFound();
        var result = await _service.GetByBusinessAsync(business.Id, status);
        return Ok(result);
    }

    [HttpPut("{id}/status"), Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request)
    {
        var result = await _service.UpdateStatusAsync(id, request.Status);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("export/csv"), Authorize]
    public async Task<IActionResult> ExportCsv()
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.UserId == GetUserId());
        if (business == null) return NotFound();
        var csv = await _service.ExportCsvAsync(business.Id);
        return File(csv, "text/csv", $"bookings_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
