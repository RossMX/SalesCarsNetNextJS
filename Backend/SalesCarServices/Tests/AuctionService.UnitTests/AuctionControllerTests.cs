using Auctions.API.Controllers;
using Auctions.API.Core;
using Auctions.API.Data;
using Auctions.API.DTOs;
using Auctions.API.Entities;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;
    private Mock<ILogger<AuctionControllerTests>> _logger;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        _logger = new Mock<ILogger<AuctionControllerTests>>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfiles).Assembly);
        });

        var mockMapper = config.CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepo.Object,
                                            _mapper,
                                            _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Return10Auctions()
    {
        // Arrange
        var auctions = _fixture.CreateMany<AuctionDTO>(10).ToList();
        _auctionRepo.Setup(r => r.GetAuctionsAsync(null)).ReturnsAsync(auctions);

        // Act
        var result = await _controller.GetAuctions(null);

        // Assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDTO>>>(result);
    }

    [Fact]
    public async Task GetAuction_WithValidGuid_ReturnAuction()
    {
        // Arrange
        var auction = _fixture.Create<AuctionDTO>();
        _auctionRepo.Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // Act
        var result = await _controller.GetAuctionById(auction.Id);

        // Assert
        Assert.Equal(auction.Make, ((AuctionDTO)((ObjectResult)result.Result).Value).Make);
        Assert.IsType<ActionResult<AuctionDTO>>(result);
    }

    [Fact]
    public async Task GetAuction_WithInvalidGuid_ReturnNotFound()
    {
        // Arrange
        _auctionRepo.Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // Act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDTO_ReturnCreatedAtAction()
    {
        // Arrange
        var auction = _fixture.Create<CreateAuctionDTO>();
        _auctionRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.CreateAuction(auction);
        var createdResult = result.Result as CreatedAtActionResult;

        // Assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDTO>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        // arrange
        var auctionDTO = _fixture.Create<CreateAuctionDTO>();
        _auctionRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

        // act
        var result = await _controller.CreateAuction(auctionDTO);

        // assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // arrange 
        var updateDTO = _fixture.Create<UpdateAuctionDTO>();

        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "test";

        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateDTO);

        // assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "not-test";

        var updateDto = _fixture.Create<UpdateAuctionDTO>();

        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        // assert 
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "test";

        var updateDto = _fixture.Create<UpdateAuctionDTO>();

        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value:null);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        // assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "test";

        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_ReturnsNotFoundResponse()
    {
        // arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();

        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_ReturnsForbidResponse()
    {
        // arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "not-test";

        _auctionRepo.Setup(r => r.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<ForbidResult>(result);
    }
}
