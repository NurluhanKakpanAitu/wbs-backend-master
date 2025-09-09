using BonusSystem.Api.Helpers;
using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos; 
using BonusSystem.Shared.Models;
using BonusSystem.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; 

namespace BonusSystem.Api.Features.Malls;

public static class MallHandlers
{
    public static async Task<IResult> GetMalls(HttpContext httpContext, IMallBffService mallBffService, [FromQuery] int page, [FromQuery] int pageSize)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
        async userId => { return await mallBffService.GetMallsAsync(page, pageSize); });
    }
    public static async Task<IResult> GetMallById(HttpContext httpContext, IMallBffService mallBffService, Guid id)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
        async userId => { return await mallBffService.GetMallByIdAsync(id); });
    }
    public static async Task<IResult> GetMallByName(HttpContext httpContext, IMallBffService mallBffService, string name)
    {
        return await RequestHelper.ProcessAuthenticatedRequest(httpContext,
        async userId => { return await mallBffService.GetMallByName(name); }); 
    } 
} 