using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class OrderItem
{
    public OrderId OrderId { get; set; }
    public AlbumId AlbumId { get; set; }
    public int Quantity { get; set; }
    public int PriceAtPurchaseInPence { get; set; }
    public Order Order { get; set; } = default!;
    public Album Album { get; set; } = default!;
}
