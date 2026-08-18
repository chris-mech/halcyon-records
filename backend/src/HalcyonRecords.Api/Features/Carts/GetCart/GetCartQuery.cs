using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Carts.GetCart;

public sealed record GetCartQuery(Guid PublicId)
    : IRequest<ErrorOr<IReadOnlyList<CartItemResponse>>>;
