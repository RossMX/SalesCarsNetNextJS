using Auctions.API.Data;
using Auctions.API.DTOs;
using Auctions.API.Entities;
using AutoMapper;
using BuildingBlocks.Messaging.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishedEndpoint;

    public AuctionsController(IAuctionRepository repository, IMapper mapper, IPublishEndpoint publishedEndpoint)
    {
        _repository = repository;
        _mapper = mapper;
        _publishedEndpoint = publishedEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAuctions(string date)
    {
        return await _repository.GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _repository.GetAuctionByIdAsync(id);

        return auction == null ? NotFound() : Ok(auction);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO createAuctionDTO)
    {
        var auction = _mapper.Map<Auction>(createAuctionDTO);

        auction.Seller = User.Identity.Name;

        _repository.AddAuction(auction);

        var auctionDTO = _mapper.Map<AuctionDTO>(auction);
        var auctionCreated = _mapper.Map<AuctionCreatedEvent>(auctionDTO);

        await _publishedEndpoint.Publish(auctionCreated);

        var result = await _repository.SaveChangesAsync();

        if (!result) return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, auctionDTO);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO dto)
    {
        var auction = await _repository.GetAuctionEntityByIdAsync(id);

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity?.Name) return Forbid();

        auction.Item.Make = dto.Make ?? auction.Item.Make;
        auction.Item.Model = dto.Model ?? auction.Item.Model;
        auction.Item.Year = dto.Year;
        auction.Item.Color = dto.Color ?? auction.Item.Color;
        auction.Item.Mileage = dto.Mileage;

        var auctionUpdated = _mapper.Map<AuctionUpdatedEvent>(auction);
        await _publishedEndpoint.Publish(auctionUpdated);

        var result = await _repository.SaveChangesAsync();

        if (!result) return BadRequest("Could not update the auction");

        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _repository.GetAuctionEntityByIdAsync(id);

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Forbid();

        _repository.RemoveAuction(auction);

        await _publishedEndpoint.Publish<AuctionDeletedEvent>(new AuctionDeletedEvent { Id = id.ToString() });

        var result = await _repository.SaveChangesAsync();

        if (!result) return BadRequest("Could not delete the auction");

        return Ok();
    }
}
