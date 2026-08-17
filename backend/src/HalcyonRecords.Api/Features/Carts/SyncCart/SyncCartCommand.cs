using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Carts.SyncCart;

public sealed record SyncCartCommand(Guid PublicId, IReadOnlyList<SyncCartItem> Items)
    : IRequest<ErrorOr<Success>>;

public sealed record SyncCartItem(string AlbumSqid, int Quantity);
