using AutoMapper;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using MongoDB.Entities;
using Search.API.Models;

namespace Search.API.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdatedEvent>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionUpdatedEvent> context)
    {
        Console.WriteLine("Consuming AuctionUpdatedEvent: " + context.Message.Id);

        var item = _mapper.Map<Item>(context.Message);

        var result = await DB.Update<Item>()
                            .Match(a => a.ID == context.Message.Id)
                            .ModifyOnly(x => new
                            {
                                x.Color,
                                x.Make,
                                x.Model,
                                x.Year,
                                x.Mileage
                            }, item)
                            .ExecuteAsync();

        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdatedEvent), 
                                    $"Problem updating item with id: {context.Message.Id} ");
    }
}

