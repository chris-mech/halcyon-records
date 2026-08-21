using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    Guid PublicId,
    string ContactFirstName,
    string ContactLastName,
    string ContactEmail,
    Guid IdempotencyKey
) : IRequest<ErrorOr<CreateOrderResponse>>;
