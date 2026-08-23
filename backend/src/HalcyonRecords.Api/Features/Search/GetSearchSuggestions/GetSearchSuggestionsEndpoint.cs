using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Search.GetSearchSuggestions;

public sealed class GetSearchSuggestionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/search/suggestions",
                async Task<Results<Ok<IReadOnlyList<string>>, ProblemHttpResult>> (
                    ISender sender
                ) =>
                {
                    ErrorOr<IReadOnlyList<string>> result = await sender.Send(
                        new GetSearchSuggestionsQuery()
                    );

                    return result.Match<Results<Ok<IReadOnlyList<string>>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<IReadOnlyList<string>>>()
                    );
                }
            )
            .WithName("GetSearchSuggestions")
            .WithTags("Search")
            .WithSummary("Get autocomplete suggestions for the search box.");
    }
}
