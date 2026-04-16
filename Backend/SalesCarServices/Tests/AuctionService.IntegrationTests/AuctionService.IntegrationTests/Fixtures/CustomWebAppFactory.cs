using Auctions.API.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests.Fixtures;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:18.3")        
        .Build();
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Configure db
            services.RemoveDbContext<AuctionDbContext>();
            services.AddDbContext<AuctionDbContext>(opt =>
            {
                opt.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });

            // Configure bus service
            services.AddMassTransitTestHarness();

            // Ensure db is initialized.
            services.EnsureCreated();

            // Configure fake JwtBearer authentication
            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                    .AddFakeJwtBearer(opt =>
                    {
                        opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                    });
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}
