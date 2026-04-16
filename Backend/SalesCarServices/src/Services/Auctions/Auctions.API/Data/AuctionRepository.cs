using Auctions.API.DTOs;
using Auctions.API.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Auctions.API.Data;

public class AuctionRepository : IAuctionRepository
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionRepository(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AuctionDTO> GetAuctionByIdAsync(Guid id)
    {
        return await _context.Auctions
                            .AsNoTracking()
                            .ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider)
                            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Auction> GetAuctionEntityByIdAsync(Guid id)
    {
        return await _context.Auctions
                            .Include(a => a.Item)
                            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AuctionDTO>> GetAuctionsAsync(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date) &&
            DateTime.TryParse(date, out var filterDate))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(filterDate.ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void AddAuction(Auction auction)
    {
        _context.Add(auction);
    }

    public void RemoveAuction(Auction auction)
    {
        _context.Remove(auction);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
