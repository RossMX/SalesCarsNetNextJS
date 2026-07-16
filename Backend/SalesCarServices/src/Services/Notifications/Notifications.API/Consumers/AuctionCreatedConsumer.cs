using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Notifications.API.Hubs;

namespace Notifications.API.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
    {
        Console.WriteLine("--> Auction created event received...");

        await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
}
