using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class Cart
{
    public CartId Id { get; set; }
    public int UserId { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = [];
}
