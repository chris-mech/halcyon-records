using System.ComponentModel;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;

public sealed class GetRelatedAlbumsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums/{sqid}/related",
                async Task<Results<Ok<IReadOnlyList<RelatedAlbumResponse>>, ProblemHttpResult>> (
                    [Description("The album's sqid, as returned by other album endpoints.")]
                        string sqid,
                    ISender sender
                ) =>
                {
                    ErrorOr<IReadOnlyList<RelatedAlbumResponse>> result = await sender.Send(
                        new GetRelatedAlbumsQuery(sqid)
                    );

                    return result.Match<
                        Results<Ok<IReadOnlyList<RelatedAlbumResponse>>, ProblemHttpResult>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<IReadOnlyList<RelatedAlbumResponse>>>()
                    );
                }
            )
            .WithName("GetRelatedAlbums")
            .WithTags("Albums")
            .WithSummary(
                "Get albums related to the given album, by shared genre, artist, or release date."
            )
            .WithDescription(
                "Fills results by priority: shared genre first, then shared artist, then closest "
                    + "release date, then a random album if slots remain. Stops once the configured "
                    + "maximum result count is reached."
            )
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("sqid", JsonValue.Create("9pXqL2"));
                    operation.SetResponseExample("404", DomainProblemDetails.Example);
                    return Task.CompletedTask;
                }
            );
    }
}
