using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;

namespace BonusSystem.Api.Infrastructure.Localization;

public sealed class XLanguageHeaderRequestCultureProvider : RequestCultureProvider
{
	private readonly string _headerName;

	public XLanguageHeaderRequestCultureProvider(string headerName = "X-Language")
	{
		_headerName = headerName;
	}

	public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
	{
		if (httpContext.Request.Headers.TryGetValue(_headerName, out StringValues values))
		{
			var value = values.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(value))
			{
				return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(value, value));
			}
		}
		return Task.FromResult<ProviderCultureResult?>(null);
	}
}


