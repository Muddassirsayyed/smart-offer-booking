using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Models;
using SmartOfferBooking.Services;

namespace SmartOfferBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _service;
    private readonly AppDbContext _db;

    public OffersController(IOfferService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetPublic([FromQuery] OfferFilterRequest filter)
    {
        var result = await _service.GetPublicOffersAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("my"), Authorize]
    public async Task<IActionResult> GetMyOffers()
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.UserId == GetUserId());
        if (business == null) return NotFound(new { message = "Business not found" });
        var result = await _service.GetByBusinessAsync(business.Id);
        return Ok(result);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateOfferRequest request)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.UserId == GetUserId());
        if (business == null) return NotFound(new { message = "Business not found" });
        var result = await _service.CreateAsync(business.Id, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOfferRequest request)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.UserId == GetUserId());
        if (business == null) return NotFound();
        var result = await _service.UpdateAsync(id, business.Id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.UserId == GetUserId());
        if (business == null) return NotFound();
        var deleted = await _service.DeleteAsync(id, business.Id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
