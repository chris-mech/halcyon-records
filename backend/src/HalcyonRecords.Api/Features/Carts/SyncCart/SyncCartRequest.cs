namespace HalcyonRecords.Api.Features.Carts.SyncCart;

public sealed record SyncCartRequest(IReadOnlyList<SyncCartItemRequest> Items);

public sealed record SyncCartItemRequest(string AlbumSqid, int Quantity);
