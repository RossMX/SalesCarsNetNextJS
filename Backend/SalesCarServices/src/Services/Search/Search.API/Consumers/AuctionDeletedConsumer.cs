using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using MongoDB.Entities;
using Search.API.Models;

namespace Search.API.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeletedEvent>
{
    public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
    {
        Console.WriteLine("Consuming AuctionDeletedEvent: " + context.Message.Id);

        //var item = await DB.Find<Item>().MatchID(context.Message.Id).ExecuteFirstAsync();
        //if (item != null)
        //{
        //    await item.DeleteAsync();
        //}

        var result = await DB.DeleteAsync<Item>(context.Message.Id);
        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionDeletedEvent),
                                    $"Problem deleting item with ID: {context.Message.Id}");
    }
}