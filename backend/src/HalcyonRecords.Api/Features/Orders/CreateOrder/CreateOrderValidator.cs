using FluentValidation;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ContactFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContactLastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
