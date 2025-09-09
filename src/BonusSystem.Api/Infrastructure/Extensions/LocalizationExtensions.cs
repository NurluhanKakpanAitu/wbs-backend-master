using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace BonusSystem.Api.Infrastructure.Extensions
{
    public static class LocalizationExtensions
    {
        private static readonly string[] SupportedCultureNames =
        {
            "ru", "en", "kk"
        };

        public static IServiceCollection AddAppLocalization(this IServiceCollection services)
        {
            services.AddLocalization();

            var supported = new[]
            {
        new CultureInfo("ru"),
        new CultureInfo("en"),
        new CultureInfo("kk")
    };

            services.Configure<RequestLocalizationOptions>(o =>
            {
                o.DefaultRequestCulture = new RequestCulture("ru");
                o.SupportedCultures = supported;
                o.SupportedUICultures = supported;

                o.RequestCultureProviders = new IRequestCultureProvider[]
                {
                    new QueryStringRequestCultureProvider { QueryStringKey = "lang" },
                    new BonusSystem.Api.Infrastructure.Localization.XLanguageHeaderRequestCultureProvider("X-Language"),
                    new CookieRequestCultureProvider()
                };

                o.FallBackToParentCultures = true;
                o.FallBackToParentUICultures = true;
            });

            return services;
        }

        public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app)
        {
            var opts = app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(opts);

            app.Use(async (ctx, next) =>
            {
                ctx.Response.OnStarting(() =>
                {
                    var ui = CultureInfo.CurrentUICulture;
                    if (!string.IsNullOrEmpty(ui.Name) && !ctx.Response.HasStarted)
                    {
                        ctx.Response.Headers["Content-Language"] = ui.Name;
                    }
                    return Task.CompletedTask;
                });
                await next();
            });

            return app;
        }

    }
}
