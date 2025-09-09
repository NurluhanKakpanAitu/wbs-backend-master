using BonusSystem.Api.Helpers;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonusSystem.Api.Features.Observers;

public static class ObserverHandlers
{
    public static async Task<IResult> GetUserContext(HttpContext httpContext, IObserverBffService observerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId => 
        {
            var context = await observerService.GetUserContextAsync(userId);
            var actions = await observerService.GetPermittedActionsAsync(userId);
            
            return new { context, actions };
        }, "Error getting user context");
    }

    public static async Task<IResult> GetStatistics(
        HttpContext httpContext,
        [FromQuery] Guid? companyId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        IObserverBffService observerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId => 
        {
            var query = new StatisticsQueryDto
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };
            
            return await observerService.GetStatisticsAsync(query);
        }, "Error getting statistics");
    }

    public static async Task<IResult> GetTransactionSummary(
        HttpContext httpContext,
        [FromQuery] Guid? companyId,
        IObserverBffService observerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId => 
        {
            return await observerService.GetTransactionSummaryAsync(companyId);
        }, "Error getting transaction summary");
    }

    public static async Task<IResult> GetCompaniesOverview(
        HttpContext httpContext,
        IObserverBffService observerService)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId => 
        {
            return await observerService.GetCompaniesOverviewAsync();
        }, "Error getting companies overview");
    }
}