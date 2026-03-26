using Auctions.API.DTOs;
using Auctions.API.Entities;
using AutoMapper;

namespace Auctions.API.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDTO>().IncludeMembers(x => x.Item);

        CreateMap<Item, AuctionDTO>();

        CreateMap<CreateAuctionDTO, Auction>()
            .ForMember(d => d.Item, o => o.MapFrom(s => s));

        CreateMap<CreateAuctionDTO, Item>();
    }
}
