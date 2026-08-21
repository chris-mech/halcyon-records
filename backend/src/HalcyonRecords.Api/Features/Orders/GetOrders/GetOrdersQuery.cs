using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using MediatR;

namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed record GetOrdersQuery(Guid PublicId, int Page, int PageSize)
    : IRequest<ErrorOr<PagedResult<OrderSummaryResponse>>>;
