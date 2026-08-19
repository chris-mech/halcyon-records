namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed record CreateOrderRequest(Guid IdempotencyKey);
