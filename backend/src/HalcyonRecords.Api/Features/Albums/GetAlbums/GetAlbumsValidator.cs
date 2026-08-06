using FluentValidation;

namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed class GetAlbumsValidator : AbstractValidator<GetAlbumsQuery>
{
    public GetAlbumsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);

        RuleForEach(x => x.Genres).MaximumLength(150);
        RuleFor(x => x.Genres)
            .Must(genres => genres!.Count <= 50)
            .WithMessage("Too many genres selected.")
            .When(x => x.Genres is not null);

        RuleFor(x => x.Sort).IsInEnum();
    }
}
