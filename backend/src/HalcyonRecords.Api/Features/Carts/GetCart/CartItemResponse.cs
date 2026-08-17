namespace HalcyonRecords.Api.Features.Carts.GetCart;

public sealed record CartItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    string? ImageUrl,
    int PriceInPence,
    int? OriginalPriceInPence,
    int Quantity,
    int UnitsInStock,
    bool IsInStock,
    IReadOnlyList<CartItemArtistResponse> Artists
);

public sealed record CartItemArtistResponse(string Sqid, string Name, string NameSlug);
