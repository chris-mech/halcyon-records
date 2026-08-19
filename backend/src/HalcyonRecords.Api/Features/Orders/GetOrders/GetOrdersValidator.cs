using FluentValidation;

namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed class GetOrdersValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 25);
    }
}
