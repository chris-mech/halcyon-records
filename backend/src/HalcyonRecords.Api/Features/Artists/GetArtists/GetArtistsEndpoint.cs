using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Artists.GetArtists;

public sealed class GetArtistsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/artists",
                async Task<Results<Ok<IReadOnlyList<ArtistListItemResponse>>, ProblemHttpResult>> (
                    ISender sender
                ) =>
                {
                    ErrorOr<IReadOnlyList<ArtistListItemResponse>> result = await sender.Send(
                        new GetArtistsQuery()
                    );

                    return result.Match<
                        Results<Ok<IReadOnlyList<ArtistListItemResponse>>, ProblemHttpResult>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<IReadOnlyList<ArtistListItemResponse>>>()
                    );
                }
            )
            .WithName("GetArtists");
    }
}
