namespace BuildingBlocks.Messaging.Contracts;

public class AuctionFinishedEvent
{
    public string AuctionId { get; set; }
    public bool ItemSold { get; set; }
    public string Winner { get; set; }
    public string Seller { get; set; }
    public int? Amount { get; set; }
}
