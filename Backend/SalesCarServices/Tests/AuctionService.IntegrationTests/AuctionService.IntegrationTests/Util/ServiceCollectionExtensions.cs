using Auctions.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

        if (descriptor != null) services.Remove(descriptor);
    }

    public static void EnsureCreated(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;
        var db = sp.GetRequiredService<AuctionDbContext>();
        db.Database.Migrate();
        DbHelper.InitDbForTest(db);
    }
}
