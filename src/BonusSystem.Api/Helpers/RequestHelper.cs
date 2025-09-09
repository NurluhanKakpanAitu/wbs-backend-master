using BonusSystem.Shared.Models;
using System.Security.Claims;

namespace BonusSystem.Api.Helpers;

/// <summary>
/// Helper class providing common request processing functionality for API endpoints
/// </summary>
public static class RequestHelper
{
    #region Authentication & Identity

    /// <summary>
    /// Extracts the user ID from the HttpContext claims
    /// </summary>
    /// <param name="httpContext">The current HTTP context</param>
    /// <returns>The user's ID if found and valid, otherwise null</returns>
    public static Guid? GetUserIdFromContext(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }

    /// <summary>
    /// Checks if the current request is from an authenticated user
    /// </summary>
    /// <param name="httpContext">The current HTTP context</param>
    /// <returns>True if the user is authenticated, otherwise false</returns>
    public static bool IsAuthenticated(HttpContext httpContext)
    {
        return httpContext.User.Identity?.IsAuthenticated == true && GetUserIdFromContext(httpContext) != null;
    }

    /// <summary>
    /// Gets the role of the current user
    /// </summary>
    /// <param name="httpContext">The current HTTP context</param>
    /// <returns>The user's role if found, otherwise null</returns>
    public static string? GetUserRole(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
    }

    #endregion

    #region Authorization & Permissions

    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    /// <param name="httpContext">The current HTTP context</param>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user has the role, otherwise false</returns>
    public static bool IsUserInRole(HttpContext httpContext, string role)
    {
        var userRole = GetUserRole(httpContext);
        return !string.IsNullOrEmpty(userRole) && userRole == role;
    }
    
    
    public static bool IsUserInAnyRole(HttpContext httpContext, params string[] roles)
    {
        var userRole = GetUserRole(httpContext);
        return !string.IsNullOrEmpty(userRole) && roles.Contains(userRole);
    }

    /// <summary>
    /// Checks if the current user is a company user, store admin, or system admin
    /// </summary>
    /// <param name="httpContext">The current HTTP context</param>
    /// <returns>True if the user has one of the admin roles, otherwise false</returns>
    public static bool IsCompanyOrAdminUser(HttpContext httpContext)
    {
        var roleClaim = GetUserRole(httpContext);
        if (string.IsNullOrEmpty(roleClaim))
        {
            return false;
        }

        return roleClaim == UserRole.Company.ToString() || 
               roleClaim == UserRole.StoreAdmin.ToString() || 
               roleClaim == UserRole.SystemAdmin.ToString();
    }

    /// <summary>
    /// Gets the company ID associated with the user
    /// </summary>
    /// <param name="httpContext">The current HTTP context</param>
    /// <param name="userId">The user's ID</param>
    /// <returns>The company ID if found, otherwise null</returns>
    public static Guid? GetCompanyIdFromUser(HttpContext httpContext, Guid userId)
    {
        // First check if there's a company ID claim - this is the primary method
        // after our new implementation adds the claim during authentication
        var companyIdClaim = httpContext.User.FindFirst("CompanyId")?.Value;
        
        if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var companyId))
        {
            return companyId;
        }
        
        // Legacy fallback: If user is a Company role, the user ID might be the company ID
        // This is for backward compatibility during the transition
        var roleClaim = GetUserRole(httpContext);
        if (roleClaim == UserRole.Company.ToString())
        {
            return userId;
        }
        
        return null;
    }

    #endregion

    #region Response Helpers

    /// <summary>
    /// Creates a success response with the provided data
    /// </summary>
    /// <param name="data">The data to include in the response</param>
    /// <returns>An HTTP 200 OK result with the data</returns>
    public static IResult CreateSuccessResponse(object data)
    {
        return Results.Ok(data);
    }

    /// <summary>
    /// Creates a success response with a simple message
    /// </summary>
    /// <param name="message">The success message</param>
    /// <returns>An HTTP 200 OK result with the message</returns>
    public static IResult CreateSuccessResponse(string message)
    {
        return Results.Ok(new
        {
            message
        });
    }

    /// <summary>
    /// Creates an error response with the provided message
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="statusCode">The HTTP status code (default: 400 Bad Request)</param>
    /// <returns>An HTTP result with the specified status code and error message</returns>
    public static IResult CreateErrorResponse(string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        return statusCode switch
        {
            StatusCodes.Status401Unauthorized => Results.Unauthorized(),
            StatusCodes.Status403Forbidden => Results.Forbid(),
            StatusCodes.Status404NotFound => Results.NotFound(new { message }),
            StatusCodes.Status500InternalServerError => Results.Problem(message),
            _ => Results.BadRequest(new { message })
        };
    } 

    /// <summary>
    /// Handles an exception by creating an appropriate error response
    /// </summary>
    /// <param name="ex">The exception to handle</param>
    /// <param name="errorContext">Additional context for the error message</param>
    /// <returns>An HTTP 500 Internal Server Error result with the error details</returns>
    public static IResult HandleExceptionResponse(Exception ex, string errorContext)
    {
        return Results.Problem($"{errorContext}: {ex.Message}");
    }

    #endregion

    #region Request Handling

    /// <summary>
    /// Processes a request with standard authentication checks
    /// </summary>
    /// <typeparam name="T">The return type of the handler function</typeparam>
    /// <param name="httpContext">The current HTTP context</param>
    /// <param name="handlerFunction">The function to execute if authenticated</param>
    /// <param name="errorContext">The context for error messages</param>
    /// <returns>The result of the handler function or an error response</returns>
    public static async Task<IResult> ProcessAuthenticatedRequest<T>(
        HttpContext httpContext,
        Func<Guid, Task<T>> handlerFunction,
        string errorContext = "Error processing request")
    {
        var userId = GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await handlerFunction(userId.Value);
            return CreateSuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleExceptionResponse(ex, errorContext);
        }
    }

    /// <summary>
    /// Processes a request with standard authentication and authorization checks
    /// </summary>
    /// <typeparam name="T">The return type of the handler function</typeparam>
    /// <param name="httpContext">The current HTTP context</param>
    /// <param name="requiredRole">The role required to access the endpoint</param>
    /// <param name="handlerFunction">The function to execute if authenticated and authorized</param>
    /// <param name="errorContext">The context for error messages</param>
    /// <returns>The result of the handler function or an error response</returns>
    public static async Task<IResult> ProcessAuthorizedRequest<T>(
        HttpContext httpContext,
        string requiredRole,
        Func<Guid, Task<T>> handlerFunction,
        string errorContext = "Error processing request")
    {
        var userId = GetUserIdFromContext(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        if (!IsUserInRole(httpContext, requiredRole))
        {
            return Results.Forbid();
        }

        try
        {
            var result = await handlerFunction(userId.Value);
            return CreateSuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleExceptionResponse(ex, errorContext);
        }
    }

    #endregion
}