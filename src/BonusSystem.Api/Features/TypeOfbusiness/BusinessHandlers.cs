
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Localization;

namespace BonusSystem.Api.Features.TypeOfbusiness;

public static class BusinessHandlers
{
    public static async Task<IResult> HandleGetTypeOfBusinessAsync(
        [FromServices] ITypeOfBusinessService typeOfBusinessService,
        CancellationToken cancellationToken)
    {
        var typeOfBusiness = await typeOfBusinessService.GetTypeOfBusinessAsync(cancellationToken);
        return typeOfBusiness is not null
            ? Results.Ok(typeOfBusiness)
            : Results.NotFound(Res.Get("Error.TypeOfBusinessNotFound"));
    }
}