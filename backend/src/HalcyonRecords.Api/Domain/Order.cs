using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class Order
{
    public OrderId Id { get; set; }
    public int UserId { get; set; }
    public required string OrderNumber { get; set; }
    public Guid IdempotencyKey { get; set; }
    public required string ContactFirstName { get; set; }
    public required string ContactLastName { get; set; }
    public required string ContactEmail { get; set; }
    public int TotalInPence { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
