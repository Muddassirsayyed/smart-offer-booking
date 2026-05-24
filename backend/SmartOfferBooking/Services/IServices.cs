using Microsoft.EntityFrameworkCore;
using SmartOfferBooking.Data;
using SmartOfferBooking.DTOs;
using SmartOfferBooking.Models;

namespace SmartOfferBooking.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
}

public interface IBusinessService
{
    Task<BusinessDto?> GetByUserIdAsync(int userId);
    Task<BusinessDto> CreateAsync(int userId, CreateBusinessRequest request);
    Task<BusinessDto?> UpdateAsync(int userId, UpdateBusinessRequest request);
}

public interface IOfferService
{
    Task<PagedResult<OfferDto>> GetPublicOffersAsync(OfferFilterRequest filter);
    Task<OfferDto?> GetByIdAsync(int id);
    Task<List<OfferDto>> GetByBusinessAsync(int businessId);
    Task<OfferDto> CreateAsync(int businessId, CreateOfferRequest request);
    Task<OfferDto?> UpdateAsync(int id, int businessId, UpdateOfferRequest request);
    Task<bool> DeleteAsync(int id, int businessId);
}

public interface ISlotService
{
    Task<List<SlotDto>> GetByOfferAsync(int offerId);
    Task<SlotDto?> GetByIdAsync(int id);
    Task<SlotDto> CreateAsync(int offerId, CreateSlotRequest request);
    Task<SlotDto?> UpdateAsync(int id, UpdateSlotRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingRequest request);
    Task<BookingDto?> GetByReferenceAsync(string reference);
    Task<List<BookingDto>> GetByBusinessAsync(int businessId, string? status);
    Task<BookingDto?> UpdateStatusAsync(int id, string status);
    Task<byte[]> ExportCsvAsync(int businessId);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int businessId);
}
