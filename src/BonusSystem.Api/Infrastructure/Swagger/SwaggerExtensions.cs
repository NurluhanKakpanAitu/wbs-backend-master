using Microsoft.OpenApi.Models;

namespace BonusSystem.Api.Infrastructure.Swagger;

public static class SwaggerExtensions
{
    /// <summary>
    /// Safely adds or updates a response description in the OpenAPI operation
    /// </summary>
    public static void EnsureResponse(this OpenApiOperation operation, string statusCode, string description)
    {
        if (!operation.Responses.ContainsKey(statusCode))
        {
            operation.Responses[statusCode] = new OpenApiResponse 
            { 
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object"
                        }
                    }
                }
            };
        }
        else
        {
            operation.Responses[statusCode].Description = description;
            
            // Ensure content is defined
            if (operation.Responses[statusCode].Content == null)
            {
                operation.Responses[statusCode].Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object"
                        }
                    }
                };
            }
        }
    }
}