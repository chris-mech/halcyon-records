using FluentValidation;

namespace HalcyonRecords.Api.Features.Carts.SyncCart;

public sealed class SyncCartValidator : AbstractValidator<SyncCartCommand>
{
    public SyncCartValidator()
    {
        RuleFor(x => x.Items).NotNull();
        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.AlbumSqid).NotEmpty();
                item.RuleFor(i => i.Quantity).GreaterThan(0);
            });
    }
}
