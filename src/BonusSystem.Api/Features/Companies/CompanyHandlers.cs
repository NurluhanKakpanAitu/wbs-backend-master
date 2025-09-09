
using BonusSystem.Api.Helpers;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using BonusSystem.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BonusSystem.Api.Features.Companies;

public static class CompanyHandlers
{
    public static async Task<IResult> GetUserContext(HttpContext httpContext, ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var context = await companyService.GetUserContextAsync(userId);
            var actions = await companyService.GetPermittedActionsAsync(userId);

            return new { context, actions };
        }, Res.Get("Error.GettingUserContext"));
    }
    public static async Task<IResult> AppointSeller(HttpContext httpContext, AppointSellerDto appointSellerDto, ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await companyService.AppointSellerAsync(companyId.Value, appointSellerDto);
        }, Res.Get("Error.AppointingSeller"));
    }
    public static async Task<IResult> RemoveSellerFromStore(HttpContext httpContext, Guid StoreId, Guid SellerId, ICompanyBffService companyBffService)
    { 
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await companyBffService.RemoveSellerForStore(StoreId, SellerId);
        }, Res.Get("Error.RemovingSellerFromStore"));
    }
    public static async Task<IResult> GetLastTransaction(HttpContext httpContext, ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await companyService.GetLastTransactionAsync(companyId);
        }, Res.Get("Error.GettingLastTransaction"));
    }
    public static async Task<IResult> ExportCompanyStoreMetrics(
        HttpContext httpContext,
        [FromQuery] Guid companyId,
        [FromQuery] string companyName,
        [FromQuery] string INN,
        [FromQuery] DateTime date,
        IMetricsBffService metricsService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyIdFromUser = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyIdFromUser == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await metricsService.ExcelExportCompanyStoreMetricsAsync(companyId, companyName, INN, date);
        }, Res.Get("Error.ExportingCompanyStoreMetrics"));
    }
    public static async Task<IResult> GetMonitoring(
        HttpContext httpContext,
        [FromQuery] string companyINN,
        [FromQuery] string companyName,
        [FromQuery] DateTime date,
        ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await companyService.GetMonitoringAsync(companyId.Value, companyINN, companyName, date);
        }, Res.Get("Error.GettingMonitoringData"));
    }
    public static async Task<IResult> GetStores(
        HttpContext httpContext,
        ICompanyBffService companyService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)

    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));
            var stores = await companyService.GetStoresForCompanyAsync(companyId.Value, page, pageSize);
            return stores;
        }, Res.Get("Error.GettingStores"));
    }
    public static async Task<IResult> GetSellers(
        HttpContext httpContext,
        ICompanyBffService companyService
    )
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException("Company ID not found for user");
            var sellers = await companyService.GetCompanySellers(companyId.Value);
            return sellers;
        }, "Error getting sellers");
    }
    public static async Task<IResult> RegisterStore(
        HttpContext httpContext,
        StoreRegistrationDto storeDto,
        ICompanyBffService companyService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);

        if (userId == null)
            return Results.Unauthorized();

        if (!RequestHelper.IsCompanyOrAdminUser(httpContext))
            return Results.Forbid();

        try
        {
            var success = await companyService.RegisterStore(storeDto);
            return success
                ? RequestHelper.CreateSuccessResponse(Res.Get("Success.StoreRegistered"))
                : Results.BadRequest(Res.Get("Error.StoreRegistrationFailed"));
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.RegisteringStore"));
        }
    }

    public static async Task<IResult> RegisterSeller(
        HttpContext httpContext,
        SellerRegistrationDto sellerDto,
        ICompanyBffService companyService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);

        if (userId == null)
            return Results.Unauthorized();

        if (!RequestHelper.IsCompanyOrAdminUser(httpContext))
            return Results.Forbid();

        try
        {
            // Get company ID from user context
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId.Value);

            if (companyId == null)
                return Results.BadRequest(Res.Get("Error.CompanyIdNotFoundForCurrentUser"));

            var seller = await companyService.RegisterSeller(sellerDto, companyId.Value);
            if (seller == null)
                return Results.BadRequest(Res.Get("Error.SellerRegistrationFailed"));
            return RequestHelper.CreateSuccessResponse(seller);
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.RegisteringSeller"));
        }
    }

    public static async Task<IResult> GetStatistics(
        HttpContext httpContext,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            var query = new StatisticsQueryDto
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            return await companyService.GetStatisticsAsync(query);
        }, Res.Get("Error.GettingCompanyStatistics"));
    }
    public static async Task<IResult> GetStoreStatistics(
        HttpContext httpContext,
        [FromBody] DashBoardStoreStatisticsDto dashBoardStoreStatisticsDto,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (!companyId.HasValue)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await companyService.GetStoreStatisticsAsync(companyId.Value, page, pageSize, dashBoardStoreStatisticsDto);
        }, Res.Get("Error.GettingStoreStatistics"));
    }
    public static async Task<IResult> GetTransactionSummary(
        HttpContext httpContext,
        ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (!companyId.HasValue)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await companyService.GetTransactionSummaryAsync(companyId.Value);
        }, Res.Get("Error.GettingTransactionSummary"));
    }
    public static async Task<IResult> GetStoresWithSellers(
        HttpContext httpContext,
        [FromBody] StoresFilterRequestDto filter,
        ICompanyBffService companyService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId.Value);
            if (companyId == null)
            {
                return Results.BadRequest(Res.Get("Error.CompanyIdNotFoundForCurrentUser"));
            }

            // Apply default values and validate filter
            if (filter == null)
            {
                filter = new StoresFilterRequestDto
                {
                    Page = 1,
                    PageSize = 10,
                    SellerRole = UserRole.Seller
                };
            }
            else
            {
                // Validate enum values
                if (filter.StoreStatus.HasValue && !Enum.IsDefined(typeof(StoreStatus), filter.StoreStatus.Value))
                {
                    return Results.BadRequest(Res.Format("Error.InvalidStoreStatusValue", filter.StoreStatus.Value));
                }

                // Sanitize pagination parameters
                if (filter.Page < 1) filter = filter with { Page = 1 };
                if (filter.PageSize < 1) filter = filter with { PageSize = 10 };
                if (filter.PageSize > 100) filter = filter with { PageSize = 100 };

            }

            var result = await companyService.GetStoresWithSellersAsync(companyId.Value, filter);
            return RequestHelper.CreateSuccessResponse(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.GettingStoresWithSellers"));
        }
    }
    public static async Task<IResult> GetFiatTransactionSummary(
        HttpContext httpContext,
        ICompanyBffService companyService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var companyId = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

            if (companyId == null)
                throw new InvalidOperationException(Res.Get("Error.CompanyIdNotFoundForUser"));

            return await companyService.GetFiatTransactionSummaryAsync(companyId);
        }, Res.Get("Error.GettingFiatTransactionSummary"));

    }
    public static async Task<IResult> DeleteStore(
        HttpContext httpContext,
        Guid storeId,
        ICompanyBffService companyService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
            return Results.Unauthorized();
        if (!RequestHelper.IsCompanyOrAdminUser(httpContext))
            return Results.Forbid();
        try
        {
            var success = await companyService.DeleteStoreByIdAsync(storeId);
            return success
                ? RequestHelper.CreateSuccessResponse(Res.Get("Success.StoreDeleted"))
                : Results.BadRequest(Res.Get("Error.StoreDeletionFailed"));
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.DeletingStore"));
        }
    }

    public static async Task<IResult> UpdateStoreAddress(
        HttpContext httpContext,
        Guid storeId,
        string newAddress, 
        string row, 
        string number, 
        string floor, 
        ICompanyBffService companyService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
            return Results.Unauthorized();
        if (!RequestHelper.IsCompanyOrAdminUser(httpContext))
            return Results.Forbid();
        try
        {
            var success = await companyService.UpdateStoreAddressAsync(storeId, newAddress, floor, number, row);
            return success
                ? RequestHelper.CreateSuccessResponse(Res.Get("Success.StoreAddressUpdated"))
                : Results.BadRequest(Res.Get("Error.StoreAddressUpdateFailed"));
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.UpdatingStoreAddress"));
        }
    }
}