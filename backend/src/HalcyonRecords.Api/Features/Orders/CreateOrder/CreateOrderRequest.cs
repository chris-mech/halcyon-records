namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed record CreateOrderRequest(
    string ContactFirstName,
    string ContactLastName,
    string ContactEmail,
    Guid IdempotencyKey
);
