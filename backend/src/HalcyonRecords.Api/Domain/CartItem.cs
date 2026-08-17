using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class CartItem
{
    public CartId CartId { get; set; }
    public AlbumId AlbumId { get; set; }
    public int Quantity { get; set; }
    public Cart Cart { get; set; } = default!;
    public Album Album { get; set; } = default!;
}
