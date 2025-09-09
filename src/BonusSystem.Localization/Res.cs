using System;
using System.Globalization;
using System.Resources;

namespace BonusSystem.Localization;

public static class Res
{
	private static readonly ResourceManager ResourceManager = new(
		"BonusSystem.Localization.Resources.Messages",
		typeof(Res).Assembly
	);

	public static string Get(string key, CultureInfo? culture = null)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return string.Empty;
		}

		var selectedCulture = culture ?? CultureInfo.CurrentUICulture;
		var value = ResourceManager.GetString(key, selectedCulture);
		return value ?? key;
	}

	public static string Format(string key, params object[] arguments)
	{
		var format = Get(key);
		return string.Format(CultureInfo.CurrentUICulture, format, arguments);
	}
}
