using FluentValidation;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
