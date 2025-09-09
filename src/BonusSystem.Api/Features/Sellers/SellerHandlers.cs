using BonusSystem.Api.Helpers;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BonusSystem.Api.Features.Sellers;

public static class SellerHandlers
{
    public static async Task<IResult> GetUserContext(HttpContext httpContext, ISellerBffService sellerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var context = await sellerService.GetUserContextAsync(userId);
            var actions = await sellerService.GetPermittedActionsAsync(userId);

            return new { context, actions };
        }, "Error getting user context");
    }
    public static async Task<IResult> GetCompanyContext(
        HttpContext httpContext,
        ISellerBffService sellerService
    )
    {
        var user = RequestHelper.GetUserIdFromContext(httpContext);
        if (user == null)
            return Results.BadRequest("User ID not found in context.");

        var company = RequestHelper.GetCompanyIdFromUser(httpContext, user.Value);
        if (company == null)
            return Results.BadRequest("Company ID not found for user.");

        var company_data = await sellerService.GetCompanyForSeller(company.Value); 
        return RequestHelper.CreateSuccessResponse(company_data);
    }
    public static async Task<IResult> GetTransactionsForBuyer(
        HttpContext httpContext,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string buyerId,
        ISellerBffService sellerService
    )
    { 
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            return await sellerService.GetBuyerTransactionsByFrontendIdAsync(userId, buyerId, page, pageSize);
        }, "Error getting transactions");
    }
    public static async Task<IResult> GetTransactionsForSeller(
        HttpContext httpContext,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ISellerBffService sellerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            return await sellerService.GetTransactionsForSeller(userId, page, pageSize);
        }, "Error getting transactions");
    }
    public static async Task<IResult> GetNotifications(HttpContext httpContext, ISellerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetNotificationsAsync(userId); },
            "Error getting notifications");
    }
    public static async Task<IResult> ProcessTransaction(
        HttpContext httpContext,
        TransactionRequestDto request,
        ISellerBffService sellerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }
        var result = await sellerService.ProcessTransactionAsync(userId.Value, request);

        return RequestHelper.CreateSuccessResponse(result);
    }

    public static async Task<IResult> ProcessFiatTransaction(
        HttpContext httpContext,
        FiatTransactionRequestDto request,
        ISellerBffService sellerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var result = await sellerService.ProcessFiatTransactionAsync(userId.Value, request);

        return RequestHelper.CreateSuccessResponse(result);
    }

    public static async Task<IResult> ProcessCombinedTransaction(
        HttpContext httpContext,
        CombinedTransactionRequestDto request,
        ISellerBffService sellerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var result = await sellerService.ProcessCombinedTransactionAsync(userId.Value, request);

        return RequestHelper.CreateSuccessResponse(result);
    }

    public static async Task<IResult> RequestTransactionReturn(
        HttpContext httpContext,
        TransactionReturnRequestDto request,
        ISellerBffService sellerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var result = await sellerService.RequestTransactionReturnAsync(userId.Value, request);

        return RequestHelper.CreateSuccessResponse(result);
    }
    public static async Task<IResult> GetTransactionReturnsRequestedBySeller(
    HttpContext httpContext,
    ISellerBffService sellerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var result = await sellerService.GetTransactionReturnsRequestedBySellerAsync(userId.Value);
        return RequestHelper.CreateSuccessResponse(result);
    }

    public static async Task<IResult> GetBuyerBalance(
        HttpContext httpContext,
        string id,
        ISellerBffService sellerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var balance = await sellerService.GetBuyerBonusBalanceAsync(id);
            return new { balance };
        }, "Error getting buyer balance");
    }

    public static async Task<IResult> GetStoreBalance(
        HttpContext httpContext,
        ISellerBffService sellerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var balance = await sellerService.GetStoreBonusBalanceByUserIdAsync(userId);
            return new { balance };
        }, "Error getting store balance");
    }
    public static async Task<IResult> GetStoreFiatBalance(
        HttpContext httpContext,
        ISellerBffService sellerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var balance = await sellerService.GetStoreFiatBalanceAsync(userId);
            return new { balance };
        }, "Error getting store fiat balance");
    }

    public static async Task<IResult> GetStoreTransactions(
        HttpContext httpContext,
        ISellerBffService sellerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            return await sellerService.GetStoreBonusTransactionsByUserIdAsync(userId);
        }, "Error getting store transactions");
    }
    public static async Task<IResult> GetStoreFiatTransactions(
        HttpContext httpContext,
        ISellerBffService sellerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId => 
        {
            return await sellerService.GetStoreFiatTransactionsByUserIdAsync(userId);
        }, "Error getting store fiat transactions");
    }
    public static async Task<IResult> CreatePendingFiatTransaction(
        HttpContext httpContext,
        FiatTransactionRequestDto request,
        ISellerBffService sellerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }
        var result = await sellerService.CreatePendingFiatTransactionAsync(userId.Value, request);
        return RequestHelper.CreateSuccessResponse(result);
    }
}