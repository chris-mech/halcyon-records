namespace HalcyonRecords.Api.Features.Orders.GetOrderByNumber;

public sealed record OrderDetailResponse(
    string OrderNumber,
    DateTimeOffset PlacedAt,
    string Status,
    int TotalInPence,
    IReadOnlyList<OrderDetailItemResponse> Items
);

public sealed record OrderDetailItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    string? ImageUrl,
    int Quantity,
    int PriceAtPurchaseInPence
);
