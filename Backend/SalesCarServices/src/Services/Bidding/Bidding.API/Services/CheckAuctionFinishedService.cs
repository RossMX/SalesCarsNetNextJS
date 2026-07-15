using Bidding.API.Models;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using MongoDB.Entities;

namespace Bidding.API.Services
{
    public class CheckAuctionFinishedService : BackgroundService
    {
        private readonly ILogger<CheckAuctionFinishedService> _logger;
        private readonly IServiceProvider _services;

        public CheckAuctionFinishedService(ILogger<CheckAuctionFinishedService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting CheckAuctionFinished background service.");

            stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAuctions(stoppingToken);
                await Task.Delay(5000, stoppingToken); // Check every 5 seconds
            }
        }

        private async Task CheckAuctions(CancellationToken stoppingToken)
        {
            // Get all the auctions that have finished but not yet marked as finished
            var finishedAuctions = await DB.Find<Auction>()
                .Match(x => x.AuctionEnd <= DateTime.UtcNow)
                .Match(x => !x.Finished)
                .ExecuteAsync(stoppingToken);

            if (finishedAuctions.Count == 0) return;

            _logger.LogInformation("==> Found {count} auctions that have finished", finishedAuctions.Count);

            using var scope = _services.CreateScope();
            var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            foreach (var auction in finishedAuctions)
            {
                auction.Finished = true;
                await auction.SaveAsync(null, stoppingToken);

                // Get the highest bid for the auction that has been accepted
                var winningBid = await DB.Find<Bid>()
                    .Match(a => a.AuctionId == auction.ID)
                    .Match(b => b.BidStatus == BidStatus.Accepted)
                    .Sort(x => x.Descending(s => s.Amount))
                    .ExecuteFirstAsync(stoppingToken);

                await endpoint.Publish(new AuctionFinishedEvent
                {
                    ItemSold = winningBid != null,
                    AuctionId = auction.ID,
                    Winner = winningBid?.Bidder,
                    Amount = winningBid?.Amount,
                    Seller = auction.Seller
                }, stoppingToken);
            }        
        }
    }
}
