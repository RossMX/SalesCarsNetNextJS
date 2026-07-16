using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Notifications.API.Hubs;

namespace Notifications.API.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        Console.WriteLine("--> Auction finished event received...");

        await _hubContext.Clients.All.SendAsync("AuctionFinished", context.Message);
    }
}
