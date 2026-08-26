using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi;

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
            .WithSummary("Get autocomplete suggestions for the search box.")
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    var schema = (OpenApiSchema)
                        operation.Responses!["200"].Content!["application/json"].Schema!;
                    schema.Examples =
                    [
                        JsonNode.Parse("""["Midnight Static", "Dream Pop", "Shoegaze"]""")!,
                    ];
                    return Task.CompletedTask;
                }
            );
    }
}
