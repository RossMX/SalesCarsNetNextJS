using BuildingBlocks.Messaging.Contracts;
using MassTransit;

namespace Auctions.API.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreatedEvent>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreatedEvent>> context)
    {
        Console.WriteLine("--> Consuming faulty creation");

        var exception = context.Message.Exceptions.First();
        if (exception.ExceptionType == "System.ArgumentException")
        {
            context.Message.Message.Model = "Foobar";
            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine("Not an argument exception");
        }
    }
}
