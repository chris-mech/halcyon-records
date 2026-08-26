using System.ComponentModel;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetAlbumById;

public sealed class GetAlbumByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums/{sqid}",
                async Task<Results<Ok<AlbumDetailResponse>, ProblemHttpResult>> (
                    [Description("The album's sqid, as returned by other album endpoints.")]
                        string sqid,
                    ISender sender
                ) =>
                {
                    ErrorOr<AlbumDetailResponse> result = await sender.Send(
                        new GetAlbumByIdQuery(sqid)
                    );

                    return result.Match<Results<Ok<AlbumDetailResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<AlbumDetailResponse>>()
                    );
                }
            )
            .WithName("GetAlbumById")
            .WithTags("Albums")
            .WithSummary("Get a single album's full detail by its sqid.")
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("sqid", JsonValue.Create("9pXqL2"));
                    return Task.CompletedTask;
                }
            );
    }
}
