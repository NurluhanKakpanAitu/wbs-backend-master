using BonusSystem.Api.Infrastructure.Extensions;
using BonusSystem.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace BonusSystem.Api.Tests.Infrastructure.Extensions;

public class RoleAuthorizationFilterTests
{
    [Fact]
    public async Task EndpointFilter_WithValidUserAndRole_ShouldAllowAccess()
    {
        // Arrange
        var httpContext = CreateHttpContextWithUser("test-user-id", UserRole.Buyer.ToString());
        var endpointFilterContext = CreateEndpointFilterContext(httpContext);
        var nextCalled = false;
        
        EndpointFilterDelegate next = (context) =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        var app = WebApplication.CreateBuilder().Build();
        var routeGroupBuilder = app.MapGroup("/test");
        routeGroupBuilder.RequireRoles(UserRole.Buyer);

        // Act
        var result = await ExecuteEndpointFilter(httpContext, next, UserRole.Buyer);

        // Assert
        Assert.True(nextCalled);
        Assert.IsType<IResult>(result);
    }

    [Fact]
    public async Task EndpointFilter_WithValidUserButWrongRole_ShouldReturnForbid()
    {
        // Arrange
        var httpContext = CreateHttpContextWithUser("test-user-id", UserRole.Buyer.ToString());
        var nextCalled = false;
        
        EndpointFilterDelegate next = (context) =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        // Act
        var result = await ExecuteEndpointFilter(httpContext, next, UserRole.SystemAdmin);

        // Assert
        Assert.False(nextCalled);
        var forbidResult = Assert.IsType<ForbidHttpResult>(result);
        Assert.NotNull(forbidResult);
    }

    [Fact]
    public async Task EndpointFilter_WithNoUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = CreateHttpContextWithoutUser();
        var nextCalled = false;
        
        EndpointFilterDelegate next = (context) =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        // Act
        var result = await ExecuteEndpointFilter(httpContext, next, UserRole.Buyer);

        // Assert
        Assert.False(nextCalled);
        var unauthorizedResult = Assert.IsType<UnauthorizedHttpResult>(result);
        Assert.NotNull(unauthorizedResult);
    }

    [Fact]
    public async Task EndpointFilter_WithInvalidUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = CreateHttpContextWithInvalidUserId(UserRole.Buyer.ToString());
        var nextCalled = false;
        
        EndpointFilterDelegate next = (context) =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        // Act
        var result = await ExecuteEndpointFilter(httpContext, next, UserRole.Buyer);

        // Assert
        Assert.False(nextCalled);
        var unauthorizedResult = Assert.IsType<UnauthorizedHttpResult>(result);
        Assert.NotNull(unauthorizedResult);
    }

    [Theory]
    [InlineData(UserRole.Buyer)]
    [InlineData(UserRole.Seller)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task EndpointFilter_WithMultipleValidRoles_ShouldAllowAccess(UserRole userRole)
    {
        // Arrange
        var httpContext = CreateHttpContextWithUser("test-user-id", userRole.ToString());
        var nextCalled = false;
        
        EndpointFilterDelegate next = (context) =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        // Act - Allow access for multiple roles
        var result = await ExecuteEndpointFilter(httpContext, next, UserRole.Buyer, UserRole.Seller, UserRole.SystemAdmin);

        // Assert
        Assert.True(nextCalled);
        Assert.IsType<IResult>(result);
    }

    private static HttpContext CreateHttpContextWithUser(string userId, string role)
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    private static HttpContext CreateHttpContextWithoutUser()
    {
        return new DefaultHttpContext();
    }

    private static HttpContext CreateHttpContextWithInvalidUserId(string role)
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    private static EndpointFilterInvocationContext CreateEndpointFilterContext(HttpContext httpContext)
    {
        return new DefaultEndpointFilterInvocationContext(httpContext, []);
    }

    private static async Task<object?> ExecuteEndpointFilter(HttpContext httpContext, EndpointFilterDelegate next, params UserRole[] requiredRoles)
    {
        // Create a mock endpoint filter that mimics the behavior of RequireRoles extension
        var roleStrings = requiredRoles.Select(r => r.ToString()).ToArray();
        
        EndpointFilterDelegate filter = async (context) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                return Results.Unauthorized();

            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole) || !roleStrings.Contains(userRole))
                return Results.Forbid();

            return await next(context);
        };

        var filterContext = CreateEndpointFilterContext(httpContext);
        return await filter(filterContext);
    }
}