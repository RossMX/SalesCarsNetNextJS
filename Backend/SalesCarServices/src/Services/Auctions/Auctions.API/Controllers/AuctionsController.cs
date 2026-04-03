using Auctions.API.Data;
using Auctions.API.DTOs;
using Auctions.API.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auctions.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishedEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishedEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishedEndpoint = publishedEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAuctions(string? date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date) &&
            DateTime.TryParse(date, out var filterDate))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(filterDate.ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();

        //var auctions = await _context.Auctions
        //                             .AsNoTracking()
        //                             .Include(a => a.Item)
        //                             .OrderBy(x => x.Item.Make)
        //                             .ToListAsync();

        //return Ok(_mapper.Map<List<AuctionDTO>>(auctions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
                                    .AsNoTracking()
                                    .Include(a => a.Item)
                                    .FirstOrDefaultAsync(a => a.Id == id);
        return auction == null 
            ? NotFound() 
            : Ok(_mapper.Map<AuctionDTO>(auction));
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO createAuctionDTO)
    {
        var auction = _mapper.Map<Auction>(createAuctionDTO);
        _context.Auctions.Add(auction);

        var auctionDTO = _mapper.Map<AuctionDTO>(auction);
        var auctionCreated = _mapper.Map<AuctionCreatedEvent>(auctionDTO);

        await _publishedEndpoint.Publish(auctionCreated);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, auctionDTO);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO dto)
    {
        var auction = await _context.Auctions
                                    .Include (a => a.Item)
                                    .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        auction.Item.Make = dto.Make ?? auction.Item.Make;
        auction.Item.Model = dto.Model ?? auction.Item.Model;
        auction.Item.Year = dto.Year ?? auction.Item.Year;
        auction.Item.Color = dto.Color ?? auction.Item.Color;
        auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;

        var auctionUpdated = _mapper.Map<AuctionUpdatedEvent>(auction);
        await _publishedEndpoint.Publish(auctionUpdated);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not update the auction");

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null) return NotFound();
        
        _context.Auctions.Remove(auction);

        await _publishedEndpoint.Publish<AuctionDeletedEvent>(new AuctionDeletedEvent { Id = id.ToString() });
        
        var result = await _context.SaveChangesAsync() > 0;
        
        if (!result) return BadRequest("Could not delete the auction");
        
        return Ok();
    }
}
