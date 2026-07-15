using AutoMapper;
using Bidding.API.DTOs;
using Bidding.API.Models;
using BuildingBlocks.Messaging.Contracts;

namespace Bidding.API.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDto>();
        CreateMap<Bid, BidPlacedEvent>();
    }
}
