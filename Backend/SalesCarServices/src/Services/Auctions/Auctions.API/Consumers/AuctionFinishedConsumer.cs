using Auctions.API.Data;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;

namespace Auctions.API.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly AuctionDbContext _dbContext;

    public AuctionFinishedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        Console.WriteLine("--> Consuming Auction finished event");

        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.Seller = context.Message.Seller;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice
            ? Entities.Status.Finished : Entities.Status.ReserveNotMet;

        await _dbContext.SaveChangesAsync();
    }
}
