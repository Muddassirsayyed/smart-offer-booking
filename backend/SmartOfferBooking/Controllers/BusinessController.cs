using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Services;

namespace SmartOfferBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _service;
    private readonly AppDbContext _db;

    public BusinessController(IBusinessService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _service.GetByUserIdAsync(GetUserId());
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequest request)
    {
        var existing = await _service.GetByUserIdAsync(GetUserId());
        if (existing != null) return Conflict(new { message = "Business already exists" });
        var result = await _service.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetMy), result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBusinessRequest request)
    {
        var result = await _service.UpdateAsync(GetUserId(), request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(int id)
    {
        var b = await _db.Businesses.FindAsync(id);
        if (b == null) return NotFound();
        return Ok(new { b.Id, b.BusinessName, b.BusinessType, b.City, b.Address, b.Phone, b.Email, b.LogoUrl });
    }
}
