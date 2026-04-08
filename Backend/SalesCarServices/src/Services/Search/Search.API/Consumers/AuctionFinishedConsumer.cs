using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using MongoDB.Entities;
using Search.API.Models;

namespace Search.API.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        Console.WriteLine("--> Consuming auction finished event");

        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
        if (auction == null) return;

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount ?? 0;
        }

        auction.Status = "Finished";

        await auction.SaveAsync();
    }
}
