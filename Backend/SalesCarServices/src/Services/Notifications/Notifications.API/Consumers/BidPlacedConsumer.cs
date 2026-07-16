using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Notifications.API.Hubs;

namespace Notifications.API.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public BidPlacedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        Console.WriteLine("--> Bid placed event received...");

        await _hubContext.Clients.All.SendAsync("BidPlaced", context.Message);
    }
}