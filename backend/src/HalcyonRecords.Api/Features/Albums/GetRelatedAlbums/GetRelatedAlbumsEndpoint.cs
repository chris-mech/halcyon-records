using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
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
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
