using BonusSystem.Api.Infrastructure.Extensions;
using BonusSystem.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace BonusSystem.Api.Tests.Infrastructure.Extensions;

public class RoleAuthorizationExtensionsTests
{
    [Fact]
    public void RequireRoles_WithValidRoles_ShouldNotThrowException()
    {
        // Arrange
        var builder = CreateMockEndpointBuilder();
        var roles = new[] { UserRole.SystemAdmin, UserRole.Buyer };

        // Act & Assert
        var exception = Record.Exception(() => builder.RequireRoles(roles));
        Assert.Null(exception);
    }

    [Fact]
    public void RequireRoles_WithNullRoles_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = CreateMockEndpointBuilder();
        UserRole[] roles = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.RequireRoles(roles));
    }

    [Fact]
    public void RequireRoles_WithEmptyRoles_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = CreateMockEndpointBuilder();
        var roles = Array.Empty<UserRole>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => builder.RequireRoles(roles));
        Assert.Contains("At least one role must be specified", exception.Message);
    }

    [Fact]
    public void RequireRoles_WithSingleRole_ShouldNotThrowException()
    {
        // Arrange
        var builder = CreateMockEndpointBuilder();
        var roles = new[] { UserRole.SystemAdmin };

        // Act & Assert
        var exception = Record.Exception(() => builder.RequireRoles(roles));
        Assert.Null(exception);
    }

    [Fact]
    public void RequireRoles_WithMultipleRoles_ShouldNotThrowException()
    {
        // Arrange
        var builder = CreateMockEndpointBuilder();
        var roles = new[] { UserRole.Buyer, UserRole.Seller, UserRole.SystemAdmin };

        // Act & Assert
        var exception = Record.Exception(() => builder.RequireRoles(roles));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(UserRole.Buyer)]
    [InlineData(UserRole.Seller)]
    [InlineData(UserRole.StoreAdmin)]
    [InlineData(UserRole.SystemAdmin)]
    [InlineData(UserRole.CompanyObserver)]
    [InlineData(UserRole.SystemObserver)]
    [InlineData(UserRole.Company)]
    public void RequireRoles_WithEachUserRole_ShouldNotThrowException(UserRole role)
    {
        // Arrange
        var builder = CreateMockEndpointBuilder();
        var roles = new[] { role };

        // Act & Assert
        var exception = Record.Exception(() => builder.RequireRoles(roles));
        Assert.Null(exception);
    }

    private static MockEndpointBuilder CreateMockEndpointBuilder()
    {
        return new MockEndpointBuilder();
    }

    private class MockEndpointBuilder : IEndpointConventionBuilder
    {
        public void Add(Action<EndpointBuilder> convention)
        {
            // Mock implementation for testing
        }

        public void Finally(Action<EndpointBuilder> finalConvention)
        {
            // Mock implementation for testing
        }
    }
}