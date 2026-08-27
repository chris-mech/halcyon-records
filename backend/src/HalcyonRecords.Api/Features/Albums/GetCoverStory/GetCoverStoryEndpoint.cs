using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetCoverStory;

public sealed class GetCoverStoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums/cover-story",
                async Task<Results<Ok<CoverStoryResponse>, ProblemHttpResult>> (ISender sender) =>
                {
                    ErrorOr<CoverStoryResponse> result = await sender.Send(
                        new GetCoverStoryQuery()
                    );

                    return result.Match<Results<Ok<CoverStoryResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<CoverStoryResponse>>()
                    );
                }
            )
            .WithName("GetCoverStory")
            .WithTags("Albums")
            .WithSummary("Get this week's featured staff-pick album.")
            .WithDescription(
                "Rotates deterministically through staff-picked albums on a weekly cycle. If no "
                    + "albums are flagged as staff picks, falls back to the newest release instead. "
                    + "Returns 404 only if the catalogue has no albums at all."
            )
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetResponseExample(
                        "404",
                        JsonNode.Parse(
                            """
                            {
                                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                                "title": "Not Found",
                                "status": 404,
                                "detail": "No albums available.",
                                "instance": "/api/albums/cover-story",
                                "code": "Album.CatalogueEmpty",
                                "traceId": "00-4f8c9b2e1a3d4c5e8f7a6b5c4d3e2f1a-9b8c7d6e5f4a3b2c-00"
                            }
                            """
                        )!
                    );
                    return Task.CompletedTask;
                }
            );
    }
}
