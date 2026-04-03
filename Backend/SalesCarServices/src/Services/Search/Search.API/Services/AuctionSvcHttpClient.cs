using MongoDB.Entities;
using Search.API.Models;

namespace Search.API.Services;

public class AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _config = config;

    public async Task<List<Item>> GetItemsFromSearchDb()
    {
        var lastUpdated = await DB.Find<Item,string>()
                                .Sort(x => x.Descending(i => i.UpdatedAt))
                                .Project(x => x.UpdatedAt.ToString())
                                .ExecuteFirstAsync();

        var baseUrl = _config["AuctionServiceBaseUrl"];
        var results = await _httpClient
                .GetFromJsonAsync<List<Item>>($"{baseUrl}/api/auctions?date={lastUpdated}");

        return results;
    }
}
