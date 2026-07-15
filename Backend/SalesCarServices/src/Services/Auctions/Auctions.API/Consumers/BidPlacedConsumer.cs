using Auctions.API.Data;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;

namespace Auctions.API.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
{
    private readonly AuctionDbContext _dbContext;

    public BidPlacedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        Console.WriteLine("--> Consuming Bid placed event");

        if (Guid.TryParse(context.Message.AuctionId, out var auctionId))
        {
            var auction = await _dbContext.Auctions.FindAsync(auctionId);

            if (auction.CurrentHighBid == null ||
                context.Message.BidStatus.Contains("Accepted") &&
                context.Message.Amount > auction.CurrentHighBid)
            {
                auction.CurrentHighBid = context.Message.Amount;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
