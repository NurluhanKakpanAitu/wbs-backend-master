using BonusSystem.Api.Features.Admin;
using BonusSystem.Api.Features.Auth;
using BonusSystem.Api.Features.Buyers;
using BonusSystem.Api.Features.City;
using BonusSystem.Api.Features.Companies;
using BonusSystem.Api.Features.Notifications;
using BonusSystem.Api.Features.Observers;
using BonusSystem.Api.Features.Regions;
using BonusSystem.Api.Features.Sellers;
using BonusSystem.Api.Features.TypeOfbusiness;
using BonusSystem.Api.Infrastructure.Extensions;
using BonusSystem.Infrastructure.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using BonusSystem.Api.Hubs;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru");


        // Configure services
        builder.Services.AddApiServices(builder.Configuration);

        // Localization
        builder.Services.AddAppLocalization();


        var app = builder.Build();

        // if (!args.Any(arg => arg.Contains("ef", StringComparison.OrdinalIgnoreCase)))
        // {
        //     using (var scope = app.Services.CreateScope())
        //     {
        //         var context = scope.ServiceProvider.GetRequiredService<BonusSystemContext>();
        //         context.Database.Migrate();
        //     }
        // }

        // Configure middleware
        app.UseAppLocalization();
        app.UseApiMiddleware(app.Environment);

        // Map endpoints by feature
        app.MapAuthEndpoints();
        app.MapRegionsEndpoints(); 
        app.MapBusinessEndpoints();
        app.MapCityEndpoints();
        app.MapBuyerEndpoints();
        app.MapSellerEndpoints();
        app.MapAdminEndpoints();
        app.MapCompanyEndpoints();
        app.MapObserverEndpoints();
        app.MapPushNotificationEndpoints();
        
        // Map SignalR Hub
        app.MapHub<CompanyStatisticsHub>("/hubs/company-statistics");

        await app.RunAsync();
    }
}
