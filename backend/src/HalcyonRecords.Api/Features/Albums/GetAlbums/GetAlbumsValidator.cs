using FluentValidation;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed class GetAlbumsValidator : AbstractValidator<GetAlbumsQuery>
{
    public GetAlbumsValidator(IOptions<AlbumsPaginationOptions> paginationOptions)
    {
        var pagination = paginationOptions.Value;

        RuleFor(x => x.Page).GreaterThanOrEqualTo(pagination.MinPage);
        RuleFor(x => x.PageSize).InclusiveBetween(pagination.MinPageSize, pagination.MaxPageSize);

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
