using Auctions.API.Data;
using AuctionService;
using Grpc.Core;

namespace Auctions.API.Services;

public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly AuctionDbContext _dbContext;

    public GrpcAuctionService(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        Console.WriteLine($"==> Received GetAuction request for AuctionId: {request.Id}");

        if (Guid.TryParse(request.Id, out var id))
        {
            var auction = await _dbContext.Auctions.FindAsync(id);
            if (auction == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Auction with Id {request.Id} not found"));

            var response = new GrpcAuctionResponse
            {
                Auction = new GrpcAuctionModel
                {
                    Id = auction.Id.ToString(),
                    Seller = auction.Seller,
                    AuctionEnd = auction.AuctionEnd.ToString(),
                    ReservePrice = auction.ReservePrice
                }
            };

            return response;
        }
        return null;
    }
}
