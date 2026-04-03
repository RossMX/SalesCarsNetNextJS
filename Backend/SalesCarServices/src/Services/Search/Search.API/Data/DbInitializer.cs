using MongoDB.Driver;
using MongoDB.Entities;
using Search.API.Models;
using Search.API.Services;

namespace Search.API.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {

        await DB.InitAsync("SearchDb",
            MongoClientSettings.FromConnectionString(
                app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();
        if (count == 0)
        {
            //var itemsData = await File.ReadAllTextAsync("Data/auctions.json");
            //var options = new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //};
            //var items = JsonSerializer.Deserialize<List<Item>>(itemsData, options);

            using var scope = app.Services.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
            var items = await client.GetItemsFromSearchDb();

            if (items != null)
                await DB.SaveAsync(items);
        }

    }
}
