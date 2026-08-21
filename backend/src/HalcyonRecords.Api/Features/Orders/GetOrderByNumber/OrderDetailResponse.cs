namespace HalcyonRecords.Api.Features.Orders.GetOrderByNumber;

public sealed record OrderDetailResponse(
    string OrderNumber,
    DateTimeOffset PlacedAt,
    string Status,
    string ContactFirstName,
    string ContactLastName,
    string ContactEmail,
    int TotalInPence,
    IReadOnlyList<OrderDetailItemResponse> Items
);

public sealed record OrderDetailItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    IReadOnlyList<OrderDetailItemArtistResponse> Artists,
    string? ImageUrl,
    int Quantity,
    int PriceAtPurchaseInPence
);

public sealed record OrderDetailItemArtistResponse(string Sqid, string Name, string NameSlug);
