using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger;

public sealed class AcceptLanguageHeaderFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
        operation.Parameters ??= new List<OpenApiParameter>();
        var alreadyExists = operation.Parameters.Any(p => string.Equals(p.Name, "X-Language", StringComparison.OrdinalIgnoreCase));
		if (alreadyExists)
		{
			return;
		}

        // Prefer X-Language for explicit switching
        operation.Parameters.Add(new OpenApiParameter
		{
            Name = "X-Language",
			In = ParameterLocation.Header,
			Required = false,
			Schema = new OpenApiSchema { Type = "string" },
            Description = "Culture code for localization (ru, en, kk). Default is ru when not provided."
		});
	}
}


