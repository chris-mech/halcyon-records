using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
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
            .WithName("Search");
    }
}
