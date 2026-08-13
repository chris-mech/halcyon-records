using FluentValidation;

namespace HalcyonRecords.Api.Features.Artists.GetArtistById;

public sealed class GetArtistByIdValidator : AbstractValidator<GetArtistByIdQuery>
{
    public GetArtistByIdValidator()
    {
        RuleFor(x => x.Sort)
            .IsEnumName(typeof(ArtistAlbumSortBy), caseSensitive: true)
            .WithMessage("'{PropertyValue}' is not a valid sort option.");
    }
}
