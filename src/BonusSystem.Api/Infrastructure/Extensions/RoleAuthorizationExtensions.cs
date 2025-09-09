using BonusSystem.Api.Helpers;
using BonusSystem.Shared.Models;

namespace BonusSystem.Api.Infrastructure.Extensions;

public static class RoleAuthorizationExtensions
{
    /// <summary>
    /// Adds role-based authorization requirement to the endpoint using UserRole enum values.
    /// </summary>
    /// <param name="builder"> The route handler builder</param>
    /// <param name="roles"> One or more UserRole enum values required to access the endpoint</param>
    /// <typeparam name="TBuilder"> The type of the endpoint convention builder</typeparam>
    /// <returns> The route handler builder for method chaining</returns>
    /// <exception cref="ArgumentException"> Thrown when no roles are specified</exception>
    public static TBuilder RequireRoles<TBuilder>(this TBuilder builder, params UserRole[] roles) 
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(roles);

        if (roles.Length == 0)
             throw new ArgumentException(@"At least one role must be specified", nameof(roles));

        var roleStrings = roles.Select(role => role.ToString()).ToArray();
        return builder.RequireRoles(roleStrings);
    }
    
    /// <summary>
    /// Adds role-based authorization requirement to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="roles"> One or more roles required to access the endpoint</param>
    /// <typeparam name="TBuilder"> The type of the endpoint convention builder</typeparam>
    /// <returns> The route handler builder for method chaining</returns>
    /// <exception cref="ArgumentException"> Thrown when no roles are specified</exception>
    private static TBuilder RequireRoles<TBuilder>(this TBuilder builder, params string[] roles) 
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(roles);

        if (roles.Length == 0)
            throw new ArgumentException(@"At least one role must be specified", nameof(roles));

        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            
            var userId = RequestHelper.GetUserIdFromContext(httpContext);
            if (userId == null)
                return Results.Unauthorized();
            
            if (!RequestHelper.IsUserInAnyRole(httpContext, roles))
                return Results.Forbid();
            
            return await next(context);
        });
    }
}