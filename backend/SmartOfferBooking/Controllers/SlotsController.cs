using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Services;

namespace SmartOfferBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _service;
    public SlotsController(ISlotService service) => _service = service;

    [HttpGet("offer/{offerId}")]
    public async Task<IActionResult> GetByOffer(int offerId)
    {
        var result = await _service.GetByOfferAsync(offerId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("offer/{offerId}"), Authorize]
    public async Task<IActionResult> Create(int offerId, [FromBody] CreateSlotRequest request)
    {
        var result = await _service.CreateAsync(offerId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSlotRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
