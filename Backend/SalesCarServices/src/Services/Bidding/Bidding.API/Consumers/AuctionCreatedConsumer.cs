using Bidding.API.Models;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using MongoDB.Entities;

namespace Bidding.API.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreatedEvent>
{
    public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
    {
        var auction = new Auction
        {
            ID = context.Message.Id.ToString(),
            Seller = context.Message.Seller,
            AuctionEnd = context.Message.AuctionEnd,
            ReservePrice = context.Message.ReservePrice
        };

        await auction.SaveAsync();
    }
}
