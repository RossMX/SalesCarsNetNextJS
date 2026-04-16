using Auctions.API.Data;
using Auctions.API.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
//public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
public class AuctionControllerTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturnThreeAuctions()
    {
        // arrange


        // act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDTO>>("api/auctions");

        // assert
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionsById_WithValidIdShouldReturnAuction()
    {
        // arrange
        var auctionId = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

        // act
        var response = await _httpClient.GetFromJsonAsync<AuctionDTO>($"api/auctions/{auctionId}");

        // assert
        Assert.Equal("GT", response.Model);
    }

    [Fact]
    public async Task GetAuctionsById_WithInvalidIdShouldReturn404()
    {
        // arrange
        var auctionId = Guid.NewGuid();

        // act
        var response = await _httpClient.GetAsync($"api/auctions/{auctionId}");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionsById_WithInvalidGuidShouldReturn400()
    {
        // arrange
        var auctionId = "not-a-valid-guid";

        // act
        var response = await _httpClient.GetAsync($"api/auctions/{auctionId}");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturn401()
    {
        // arrange
        var auction = new CreateAuctionDTO
        {
            Make = "Test"
        };

        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturn201()
    {
        // arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDTO>();
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        // arrange
        var auction = GetAuctionForCreate();
        auction.Make = null;
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        // arrange
        var auctionId = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
        var updatedAuction = new UpdateAuctionDTO { Make = "Updated" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{auctionId}", updatedAuction);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange
        var auctionId = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
        var updatedAuction = GetUpdateAuctionDTO();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("tom"));

        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{auctionId}", updatedAuction);

        // assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(db);
        return Task.CompletedTask;
    }

    private CreateAuctionDTO GetAuctionForCreate()
    {
        return new CreateAuctionDTO
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "test",
            Color = "test",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10,
            AuctionEnd = DateTime.UtcNow.AddDays(10)
        };
    }

    private UpdateAuctionDTO GetUpdateAuctionDTO()
    {
        return new UpdateAuctionDTO
        {
            Make = "Ford",
            Model = "GT",
            Color = "White",
            Mileage = 50000,
            Year = 2020,
        };
    }
}
