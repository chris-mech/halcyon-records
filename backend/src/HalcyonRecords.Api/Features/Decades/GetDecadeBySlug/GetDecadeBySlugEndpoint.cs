using System.ComponentModel;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Decades.GetDecadeBySlug;

public sealed class GetDecadeBySlugEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/decades/{slug}",
                async Task<Results<Ok<DecadeDetailResponse>, ProblemHttpResult>> (
                    [Description("The decade's slug, as returned by the decades list endpoint.")]
                        string slug,
                    ISender sender
                ) =>
                {
                    ErrorOr<DecadeDetailResponse> result = await sender.Send(
                        new GetDecadeBySlugQuery(slug)
                    );

                    return result.Match<Results<Ok<DecadeDetailResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<DecadeDetailResponse>>()
                    );
                }
            )
            .WithName("GetDecadeBySlug")
            .WithTags("Decades")
            .WithSummary("Get a single decade's detail by its slug.")
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("slug", JsonValue.Create("1990s"));
                    operation.SetResponseExample(
                        "404",
                        JsonNode.Parse(
                            """
                            {
                                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                                "title": "Not Found",
                                "status": 404,
                                "detail": "Decade '1990s' was not found.",
                                "instance": "/api/decades/1990s",
                                "code": "Decade.NotFound",
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
