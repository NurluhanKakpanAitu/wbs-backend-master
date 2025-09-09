using BonusSystem.Api.Helpers;
using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos; 
using BonusSystem.Shared.Models;
using BonusSystem.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BonusSystem.Api.Features.Buyers;

public static class BuyerHandlers
{
    public static async Task<IResult> GetUserContext(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var context = await buyerService.GetUserContextAsync(userId);
            var actions = await buyerService.GetPermittedActionsAsync(userId);

            return new { context, actions };
        }, Res.Get("Error.GettingUserContext"));
    } 
    public static async Task<IResult> GetDateBonusRemoved(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetDateBonusRemove(userId); },
            Res.Get("Error.GettingDateBonusRemoved"));
    } 
    public static async Task<IResult> GetAllTransactions(HttpContext httpContext, IBuyerBffService buyerService, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var transactions = await buyerService.AllTransactionsForUser(userId, page, pageSize);
            return Results.Ok(transactions);
        }, Res.Get("Error.GettingTransactions"));
    }
    public static async Task<IResult> GetTransfers(HttpContext httpContext, IBuyerBffService buyerService, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetTransfersAsync(userId, page, pageSize); },
            Res.Get("Error.GettingTransfers"));
            
    }
    public static async Task<IResult> GetReplenishments(HttpContext httpContext, IBuyerBffService buyerService, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetReplenishmentsForBuyerAsync(userId, page, pageSize); },
            Res.Get("Error.GettingTransfers"));
    }
    public static async Task<IResult> GetNotifications(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetNotificationsAsync(userId); },
            Res.Get("Error.GettingNotifications"));
    }
    public static async Task<IResult> GetBalance(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetBonusSummaryAsync(userId); }, Res.Get("Error.GettingBalance"));
    }
    public static async Task<IResult> GetFiatBalance(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetFiatBalanceAsync(userId); },
            Res.Get("Error.GettingFiatBalance"));
    }
    public static async Task<IResult> TransferBonus(
        HttpContext httpContext,
        IBuyerBffService buyerService,
        TransferForm transfer)
    { 
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.TransferBonusesAsync(userId, transfer); },
            Res.Get("Error.TransferringBonus"));
    }
    
    public static async Task<IResult> GetTransactions(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetTransactionHistoryAsync(userId); },
            Res.Get("Error.GettingTransactions"));
    }
    public static async Task<IResult> GetFiatTransactions(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetFiatTransactionHistoryAsync(userId); },
            Res.Get("Error.GettingFiatTransactions"));
    }
    public static async Task<IResult> GenerateQrCode(HttpContext httpContext, IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var qrCode = await buyerService.GenerateQrCodeAsync(userId);
            return new { qrCode };
        }, Res.Get("Error.GeneratingQrCode"));
    }
    public static async Task<IResult> ReplenishingWallet(HttpContext httpContext, IBuyerBffService buyerService, decimal amount)
    { 
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.ReplenishWalletAsync(userId, amount); },
            Res.Get("Error.ReplenishingWallet"));
    } 
    public static async Task<IResult> GetCategories(HttpContext httpContext, IBuyerBffService buyerService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
            async userId => { return await buyerService.GetCategoriesAsync(page, pageSize); },
            Res.Get("Error.GettingCategories"));
    }
    public static async Task<IResult> FindStores(
        [FromQuery] int categoryId,
        [FromServices] IBuyerBffService buyerService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new StoreFilterRequestDto
        {
            CategoryId = categoryId,
            Page = page,
            PageSize = pageSize
        };
        var result = await buyerService.FindStoresAsync(filter);
        return Results.Ok(result);
    }
    public static async Task<IResult> GetPendingReturnsToApprove(
    HttpContext httpContext,
    IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var pendingReturns = await buyerService.GetPendingReturnsToApproveAsync(userId);
            return pendingReturns;
        }, Res.Get("Error.RetrievingPendingReturns"));
    }

    public static async Task<IResult> GetApprovedReturns(
    HttpContext httpContext,
    IBuyerBffService buyerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
        {
            var approvedReturns = await buyerService.GetApprovedReturnsByUserAsync(userId);
            return approvedReturns;
        }, Res.Get("Error.RetrievingApprovedReturns"));
    }

    public static async Task<IResult> ApproveFiatTransactionReturn(
    HttpContext httpContext,
    Guid fiatTransactionId,
    IBuyerBffService buyerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await buyerService.ApproveFiatTransactionReturnAsync(userId.Value, fiatTransactionId);
            if (!success)
            {
                return Results.BadRequest(Res.Get("Error.TransactionReturnApproveFailed"));
            }

            return RequestHelper.CreateSuccessResponse(Res.Get("Success.FiatTransactionReturnApproved"));
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.ApprovingFiatTransactionReturn"));
        }
    }
    public static async Task<IResult> ApproveBonusTransactionReturn(
    HttpContext httpContext,
    Guid transactionId,
    
    IBuyerBffService buyerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await buyerService.ApproveBonusTransactionReturnAsync(userId.Value, transactionId);
            if (!success)
            {
                return Results.BadRequest(Res.Get("Error.TransactionReturnApproveFailed"));
            }

            return RequestHelper.CreateSuccessResponse(Res.Get("Success.BonusTransactionReturnApproved"));
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.ApprovingBonusTransactionReturn"));
        }
    }

    public static async Task<IResult> RejectTransactionReturn(
    HttpContext httpContext,
    Guid returnId,
    IBuyerBffService buyerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await buyerService.RejectTransactionReturnAsync(userId.Value, returnId);
            if (!success)
            {
                return Results.BadRequest(Res.Get("Error.TransactionReturnRejectFailed"));
            }

            return RequestHelper.CreateSuccessResponse(Res.Get("Success.TransactionReturnRejected"));
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.RejectingTransactionReturn"));
        }
    }

    public static async Task<IResult> ConfirmPendingFiatTransaction(
        HttpContext httpContext,
        Guid fiatTransactionId,
        IBuyerBffService buyerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }
        
        try
        {
            var success = await buyerService.ConfirmPendingFiatTransactionAsync(userId.Value, fiatTransactionId);
            if (!success)
            {
                return Results.BadRequest(Res.Get("Error.FiatTransactionConfirmFailed"));
            }
            return RequestHelper.CreateSuccessResponse(Res.Get("Success.FiatTransactionConfirmed"));
        }
        catch (Exception ex)
        {
            return RequestHelper.CreateErrorResponse(Res.Format("Error.ConfirmingFiatTransaction", ex.Message));
        }
    }
    
    public static async Task<IResult> ConfirmPendingBonusTransaction(
        HttpContext httpContext,
        Guid transactionId,
        IBuyerBffService buyerService)
    {
        var userId = RequestHelper.GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }
        try
        {
            var success = await buyerService.ConfirmPendingBonusTransactionAsync(userId.Value, transactionId);
            if (!success)
            {
                return Results.BadRequest(Res.Get("Error.BonusTransactionConfirmFailed"));
            }
            return RequestHelper.CreateSuccessResponse(Res.Get("Success.BonusTransactionConfirmed"));
        }
        catch (Exception ex)
        {
            return RequestHelper.CreateErrorResponse(Res.Format("Error.ConfirmingBonusTransaction", ex.Message));
        }
    }
}