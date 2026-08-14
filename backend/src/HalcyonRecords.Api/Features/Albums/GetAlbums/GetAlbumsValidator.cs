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

        RuleFor(x => x.Sort)
            .IsEnumName(typeof(AlbumSortBy), caseSensitive: true)
            .WithMessage("'{PropertyValue}' is not a valid sort option.");

        RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .WithMessage("'EndYear' must be greater than or equal to 'StartYear'.")
            .When(x => x.StartYear is not null && x.EndYear is not null);
    }
}
