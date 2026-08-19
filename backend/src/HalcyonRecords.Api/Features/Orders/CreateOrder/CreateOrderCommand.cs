using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed record CreateOrderCommand(Guid PublicId, Guid IdempotencyKey)
    : IRequest<ErrorOr<CreateOrderResponse>>;
