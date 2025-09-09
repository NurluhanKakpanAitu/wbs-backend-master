using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger.Documentation;

/// <summary>
/// Configures custom Swagger/OpenAPI options
/// </summary>
public class SwaggerConfigurationOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        // Configure response content types for all endpoints
        options.UseAllOfToExtendReferenceSchemas();
        
        // Add global response types
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Configure response media types - ensure all endpoints can return JSON
        options.MapType<object>(() => new OpenApiSchema 
        { 
            Type = "object",
            Nullable = false
        });
        
        // Add response examples at the application level
        options.ExampleFilters();
    }
}