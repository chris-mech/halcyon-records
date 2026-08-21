using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Orders.GetOrderByNumber;

public sealed record GetOrderByNumberQuery(Guid PublicId, string OrderNumber)
    : IRequest<ErrorOr<OrderDetailResponse>>;
