using FluentValidation;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed class GetOrdersValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersValidator(IOptions<OrdersPaginationOptions> paginationOptions)
    {
        var pagination = paginationOptions.Value;

        RuleFor(x => x.Page).GreaterThanOrEqualTo(pagination.MinPage);
        RuleFor(x => x.PageSize).InclusiveBetween(pagination.MinPageSize, pagination.MaxPageSize);
    }
}
