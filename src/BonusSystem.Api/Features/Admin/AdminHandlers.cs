using BonusSystem.Api.Helpers;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using BonusSystem.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BonusSystem.Api.Features.Admin;

public static class AdminHandlers
{
    public static async Task<IResult> GetUserContext(HttpContext httpContext, IAdminBffService adminService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var context = await adminService.GetUserContextAsync(userId);
            var actions = await adminService.GetPermittedActionsAsync(userId);

            return new { context, actions };
        }, Res.Get("Error.GettingUserContext"));
    }
    public static async Task<IResult> UpdateCompany(
        HttpContext httpContext,
        Guid companyId, 
        CompanyUpdateDto companyUpdateDto,
        ICompanyBffService companyBff
    )
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userid =>
        {
            await companyBff.UpdateCompanyAsync(companyId, companyUpdateDto);
            return Res.Get("Success.CompanyUpdated"); 
        }, Res.Get("Error.UpdatingCompany")); 
    }
    public static async Task<IResult> ReplenishmentFiatCompany(
        HttpContext httpContext,
        Guid companyId,
        decimal amount,
        IAdminBffService companyBff
    )
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            await companyBff.ReplenishFiatCompanyAsync(companyId, amount);
            return Res.Get("Success.CompanyFiatBalanceReplenished");
        }, Res.Get("Error.ReplenishingCompanyFiatBalance"));
    }
    public static async Task<IResult> AppointAdmin(
        HttpContext httpContext,
        AppointAdminDto appointAdminDto,
        IAdminBffService adminBffService
    )
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userid =>
        {
            await adminBffService.AppointAdminAsync(appointAdminDto);
            return Res.Get("Success.AdminAppointed");
        }, Res.Get("Error.AppointingAdmin"));
    }
    
    public static async Task<IResult> SearchCompany(
        HttpContext httpContext,
        [FromQuery] string? name,
        [FromQuery] string? bin,
        [FromQuery] string? Number_of_contract,
        [FromQuery] string? inn,
        ICompanyBffService companyservice)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var searchCriteria = new FindCompanyDto
            {
                Name = name,
                BIN = bin,
                Number_of_contract = Number_of_contract,
                INN = inn
            };
            var company = await companyservice.FindContractAsync(searchCriteria);
            return company;
        }, Res.Get("Error.SearchingCompanies"));
    }
    public static async Task<IResult> SendCompanyUrl(
        Guid id,
        IAdminBffService adminBffService
    )
    { 
        var result = await adminBffService.SendCompanyURLAsync(id);
        if (!result.Success)
            return Results.BadRequest(new { error = result.ErrorMessage });
        return RequestHelper.CreateSuccessResponse(new
        {
            userId = result.UserId,
            token = result.Token,
            role = result.Role
        });
   
    }
    public static async Task<IResult> RegisterCompany(
        HttpContext httpContext,
        CompanyRegistrationDto request,
        IAdminBffService adminService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var result = await adminService.RegisterCompanyAsync(request);
            if (!result.Success)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }

            return result;
        }, Res.Get("Error.RegisteringCompany"));
    }

    public static async Task<IResult> UpdateCompanyStatus(
        HttpContext httpContext,
        Guid id,
        [FromBody] CompanyStatus status,
        IAdminBffService adminService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await adminService.UpdateCompanyStatusAsync(id, status);
            if (!success)
            {
                return Results.BadRequest(new { error = Res.Get("Error.CompanyStatusUpdateFailed") });
            }

            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.CompanyStatusUpdated") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.UpdatingCompanyStatus"));
        }
    }

    public static async Task<IResult> ModerateStore(
        HttpContext httpContext,
        Guid id,
        [FromQuery] bool approve,
        IAdminBffService adminService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await adminService.ModerateStoreAsync(id, approve);
            if (!success)
            {
                return Results.BadRequest(new { error = Res.Get("Error.StoreModerationFailed") });
            }

            return RequestHelper.CreateSuccessResponse(new { message = approve ? Res.Get("Success.StoreApproved") : Res.Get("Success.StoreRejected") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.ModeratingStore"));
        }
    }

    public static async Task<IResult> CreditCompanyBalance(
        HttpContext httpContext,
        Guid id,
        decimal amount,
        IAdminBffService adminService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await adminService.CreditCompanyBalanceAsync(id, amount);
            if (!success)
            {
                return Results.BadRequest(new { error = Res.Get("Error.CompanyBalanceCreditFailed") });
            }

            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.CompanyBalanceCredited") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.CreditingCompanyBalance"));
        }
    }

    public static async Task<IResult> GetSystemTransactions(
        HttpContext httpContext,
        [FromQuery] Guid? companyId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        IAdminBffService adminService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            return await adminService.GetSystemTransactionsAsync(companyId, startDate, endDate);
        }, Res.Get("Error.GettingSystemTransactions"));
    }

    public static async Task<IResult> SendSystemNotification(
        HttpContext httpContext,
        [FromQuery] Guid? recipientId,
        [FromBody] NotificationDto request, 
        IAdminBffService adminService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await adminService.SendSystemNotificationAsync(recipientId, request.Message, request.Type);
            if (!success)
            {
                return Results.BadRequest(new { error = Res.Get("Error.NotificationSendFailed") });
            }

            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.NotificationSent") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.SendingNotification"));
        }
    }

    public static async Task<IResult> GetTransactionFeeReport(
        HttpContext httpContext,
        TransactionFeeRequest request,
        IAdminBffService adminBffService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            return await adminBffService.GetTransactionFeesAsync(request);
        }, Res.Get("Error.FeeCalculationQuery"));
    }
    public static async Task<IResult> GetSystemFiatTransactions(
        HttpContext httpContext,
        [FromQuery] Guid? companyId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        IAdminBffService adminService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId => 
        {
            return await adminService.GetSystemFiatTransactionsAsync(companyId, startDate, endDate);
        }, Res.Get("Error.GettingSystemFiatTransactions"));
    }
}
