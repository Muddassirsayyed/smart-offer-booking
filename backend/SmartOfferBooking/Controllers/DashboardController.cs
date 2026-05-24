using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfferBooking.Data;
using SmartOfferBooking.Services;

namespace SmartOfferBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private readonly AppDbContext _db;

    public DashboardController(IDashboardService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.UserId == GetUserId());
        if (business == null) return NotFound(new { message = "Business not found" });
        var result = await _service.GetSummaryAsync(business.Id);
        return Ok(result);
    }
}
