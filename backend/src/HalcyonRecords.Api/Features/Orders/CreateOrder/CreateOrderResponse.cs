namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed record CreateOrderResponse(
    string OrderNumber,
    DateTimeOffset PlacedAt,
    int TotalInPence,
    IReadOnlyList<CreateOrderItemResponse> Items
);

public sealed record CreateOrderItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    string? ImageUrl,
    int Quantity,
    int PriceAtPurchaseInPence
);
