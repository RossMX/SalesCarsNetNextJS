using AutoMapper;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using MongoDB.Entities;
using Search.API.Models;

namespace Search.API.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreatedEvent>
{
    private readonly IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
    {
        Console.WriteLine("Consuming AuctionCreatedEvent: " + context.Message.Id);

        var item = _mapper.Map<Item>(context.Message);
        if (item.Model == "Foo")
        {
            Console.WriteLine(" Exception: Cannot sell cars with model: Foo");
            throw new ArgumentException("Cannot sell cars with name of Foo");
        }

        await item.SaveAsync();
    }
}
