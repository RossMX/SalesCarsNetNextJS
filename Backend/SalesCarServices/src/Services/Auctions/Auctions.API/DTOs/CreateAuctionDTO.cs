using System.ComponentModel.DataAnnotations;

namespace Auctions.API.DTOs;

public class CreateAuctionDTO
{
    [Required]
    public int ReservePrice { get; set; }

    public string? Seller { get; set; }
    
    [Required]
    public DateTime AuctionEnd { get; set; }

    // Item (Car) properties
    [Required]
    public string Make { get; set; }
    [Required]
    public string Model { get; set; }
    [Required]
    public int Year { get; set; }
    [Required]
    public string Color { get; set; }
    [Required]
    public int Mileage { get; set; }
    [Required]
    public string ImageUrl { get; set; }
}
