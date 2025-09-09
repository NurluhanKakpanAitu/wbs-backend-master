using BonusSystem.Api.Helpers;
using BonusSystem.Shared.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;

namespace BonusSystem.Api.Tests.Helpers;

public class RequestHelperTests
{
    #region GetUserIdFromContext Tests

    [Fact]
    public void GetUserIdFromContext_WithValidUserId_ShouldReturnGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), nameof(UserRole.Buyer));

        // Act
        var result = RequestHelper.GetUserIdFromContext(httpContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Value);
    }

    [Fact]
    public void GetUserIdFromContext_WithInvalidUserId_ShouldReturnNull()
    {
        // Arrange
        var httpContext = CreateHttpContextWithInvalidUserId();

        // Act
        var result = RequestHelper.GetUserIdFromContext(httpContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserIdFromContext_WithNoUserId_ShouldReturnNull()
    {
        // Arrange
        var httpContext = CreateHttpContextWithoutUser();

        // Act
        var result = RequestHelper.GetUserIdFromContext(httpContext);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region IsAuthenticated Tests

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUserAndValidId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), UserRole.Buyer.ToString());

        // Act
        var result = RequestHelper.IsAuthenticated(httpContext);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_WithUnauthenticatedUser_ShouldReturnFalse()
    {
        // Arrange
        var httpContext = CreateHttpContextWithoutUser();

        // Act
        var result = RequestHelper.IsAuthenticated(httpContext);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUserButInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var httpContext = CreateHttpContextWithInvalidUserId();

        // Act
        var result = RequestHelper.IsAuthenticated(httpContext);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetUserRole Tests

    [Theory]
    [InlineData(UserRole.Buyer)]
    [InlineData(UserRole.Seller)]
    [InlineData(UserRole.SystemAdmin)]
    [InlineData(UserRole.StoreAdmin)]
    [InlineData(UserRole.CompanyObserver)]
    [InlineData(UserRole.SystemObserver)]
    [InlineData(UserRole.Company)]
    public void GetUserRole_WithValidRole_ShouldReturnCorrectRole(UserRole expectedRole)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), expectedRole.ToString());

        // Act
        var result = RequestHelper.GetUserRole(httpContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRole.ToString(), result);
    }

    [Fact]
    public void GetUserRole_WithNoRole_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUserButNoRole(userId.ToString());

        // Act
        var result = RequestHelper.GetUserRole(httpContext);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region IsUserInRole Tests

    [Theory]
    [InlineData(UserRole.Buyer)]
    [InlineData(UserRole.Seller)]
    [InlineData(UserRole.SystemAdmin)]
    public void IsUserInRole_WithMatchingRole_ShouldReturnTrue(UserRole userRole)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), userRole.ToString());

        // Act
        var result = RequestHelper.IsUserInRole(httpContext, userRole.ToString());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUserInRole_WithNonMatchingRole_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), UserRole.Buyer.ToString());

        // Act
        var result = RequestHelper.IsUserInRole(httpContext, UserRole.SystemAdmin.ToString());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUserInRole_WithNoRole_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUserButNoRole(userId.ToString());

        // Act
        var result = RequestHelper.IsUserInRole(httpContext, UserRole.Buyer.ToString());

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IsUserInAnyRole Tests

    [Fact]
    public void IsUserInAnyRole_WithMatchingRole_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), UserRole.Buyer.ToString());
        var roles = new[] { UserRole.Seller.ToString(), UserRole.Buyer.ToString(), UserRole.SystemAdmin.ToString() };

        // Act
        var result = RequestHelper.IsUserInAnyRole(httpContext, roles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUserInAnyRole_WithNoMatchingRole_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), UserRole.CompanyObserver.ToString());
        var roles = new[] { UserRole.Seller.ToString(), UserRole.Buyer.ToString(), UserRole.SystemAdmin.ToString() };

        // Act
        var result = RequestHelper.IsUserInAnyRole(httpContext, roles);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUserInAnyRole_WithEmptyRoles_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), UserRole.Buyer.ToString());
        var roles = Array.Empty<string>();

        // Act
        var result = RequestHelper.IsUserInAnyRole(httpContext, roles);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUserInAnyRole_WithNoUserRole_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUserButNoRole(userId.ToString());
        var roles = new[] { UserRole.Buyer.ToString(), UserRole.Seller.ToString() };

        // Act
        var result = RequestHelper.IsUserInAnyRole(httpContext, roles);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IsCompanyOrAdminUser Tests

    [Theory]
    [InlineData(UserRole.Company)]
    [InlineData(UserRole.StoreAdmin)]
    [InlineData(UserRole.SystemAdmin)]
    public void IsCompanyOrAdminUser_WithAdminRoles_ShouldReturnTrue(UserRole adminRole)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), adminRole.ToString());

        // Act
        var result = RequestHelper.IsCompanyOrAdminUser(httpContext);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(UserRole.Buyer)]
    [InlineData(UserRole.Seller)]
    [InlineData(UserRole.CompanyObserver)]
    [InlineData(UserRole.SystemObserver)]
    public void IsCompanyOrAdminUser_WithNonAdminRoles_ShouldReturnFalse(UserRole nonAdminRole)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), nonAdminRole.ToString());

        // Act
        var result = RequestHelper.IsCompanyOrAdminUser(httpContext);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCompanyOrAdminUser_WithNoRole_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUserButNoRole(userId.ToString());

        // Act
        var result = RequestHelper.IsCompanyOrAdminUser(httpContext);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetCompanyIdFromUser Tests

    [Fact]
    public void GetCompanyIdFromUser_WithCompanyIdClaim_ShouldReturnCompanyId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUserAndCompany(userId.ToString(), UserRole.Buyer.ToString(), companyId.ToString());

        // Act
        var result = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(companyId, result.Value);
    }

    [Fact]
    public void GetCompanyIdFromUser_WithCompanyRole_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), UserRole.Company.ToString());

        // Act
        var result = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Value);
    }

    [Fact]
    public void GetCompanyIdFromUser_WithNoCompanyIdAndNonCompanyRole_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUser(userId.ToString(), UserRole.Buyer.ToString());

        // Act
        var result = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCompanyIdFromUser_WithInvalidCompanyIdClaim_ShouldFallbackToRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithUserAndInvalidCompany(userId.ToString(), UserRole.Company.ToString(), "invalid-company-id");

        // Act
        var result = RequestHelper.GetCompanyIdFromUser(httpContext, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Value);
    }

    #endregion

    #region Helper Methods

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

    private static HttpContext CreateHttpContextWithUserAndCompany(string userId, string role, string companyId)
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role),
            new Claim("CompanyId", companyId)
        };
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    private static HttpContext CreateHttpContextWithUserAndInvalidCompany(string userId, string role, string invalidCompanyId)
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role),
            new Claim("CompanyId", invalidCompanyId)
        };
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    private static HttpContext CreateHttpContextWithUserButNoRole(string userId)
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    private static HttpContext CreateHttpContextWithoutUser()
    {
        return new DefaultHttpContext();
    }

    private static HttpContext CreateHttpContextWithInvalidUserId()
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid"),
            new Claim(ClaimTypes.Role, UserRole.Buyer.ToString())
        };
        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    #endregion
}