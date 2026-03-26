using Auctions.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auctions.API.Data;

public class AuctionDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Item> Items { get; set; }
}
