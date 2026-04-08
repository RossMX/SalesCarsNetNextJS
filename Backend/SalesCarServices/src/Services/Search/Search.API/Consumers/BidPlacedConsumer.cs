using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using MongoDB.Entities;
using Search.API.Models;

namespace Search.API.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
{
    public async Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        Console.WriteLine("--> Consuming bid placed event");
        
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
        if (auction == null) return;

        if (context.Message.BidStatus.Contains("Accepted") &&
            context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}
