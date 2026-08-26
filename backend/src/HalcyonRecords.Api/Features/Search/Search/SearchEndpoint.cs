using System.ComponentModel;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Search.Search;

public sealed class SearchEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/search",
                async Task<Results<Ok<SearchResponse>, ProblemHttpResult, ValidationProblem>> (
                    [Description(
                        "The free-text search query. Required; a missing or empty query fails validation."
                    )]
                        string? q,
                    ISender sender
                ) =>
                {
                    ErrorOr<SearchResponse> result = await sender.Send(
                        new SearchQuery(q ?? string.Empty)
                    );

                    return result.Match<
                        Results<Ok<SearchResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.ProblemWithValidationProblem<Ok<SearchResponse>>()
                    );
                }
            )
            .WithName("Search")
            .WithTags("Search")
            .WithSummary("Search across albums, artists, and genres by free-text query.")
            .WithDescription(
                "When the query has best matches, returns them in bestMatches along with related "
                    + "albums in suggestions, and suggestedTerms is empty. When there are no best "
                    + "matches, bestMatches and suggestions are both empty and suggestedTerms "
                    + "offers alternative search terms instead."
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("q", JsonValue.Create("Midnight Static"));
                    return Task.CompletedTask;
                }
            );
    }
}
