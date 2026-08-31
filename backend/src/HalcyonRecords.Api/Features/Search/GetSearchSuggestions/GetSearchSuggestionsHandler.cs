using ErrorOr;
using HalcyonRecords.Api.Infrastructure.Sql;
using MediatR;

namespace HalcyonRecords.Api.Features.Search.GetSearchSuggestions;

public sealed class GetSearchSuggestionsHandler(
    SuggestedTermsProvider suggestedTermsProvider,
    ApplicationDbContext dbContext
) : IRequestHandler<GetSearchSuggestionsQuery, ErrorOr<IReadOnlyList<string>>>
{
    public async Task<ErrorOr<IReadOnlyList<string>>> Handle(
        GetSearchSuggestionsQuery query,
        CancellationToken cancellationToken
    )
    {
        var terms = await suggestedTermsProvider.GetRandomTermsAsync(dbContext, cancellationToken);
        return await ErrorOrFactory.FromAsync(terms);
    }
}
