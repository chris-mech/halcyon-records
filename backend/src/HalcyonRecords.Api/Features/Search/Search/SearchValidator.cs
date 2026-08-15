using FluentValidation;

namespace HalcyonRecords.Api.Features.Search.Search;

public sealed class SearchValidator : AbstractValidator<SearchQuery>
{
    public SearchValidator()
    {
        RuleFor(x => x.Q).NotEmpty().WithMessage("A search query is required.");
    }
}
