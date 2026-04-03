using AutoMapper;
using BuildingBlocks.Messaging.Contracts;
using Search.API.Models;

namespace Search.API.Core;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuctionCreatedEvent, Item>();

        CreateMap<AuctionUpdatedEvent, Item>();
    }
}
