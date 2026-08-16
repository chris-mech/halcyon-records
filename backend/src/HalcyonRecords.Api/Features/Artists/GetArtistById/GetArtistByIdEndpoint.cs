using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Artists.GetArtistById;

public sealed class GetArtistByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/artists/{sqid}",
                async Task<
                    Results<Ok<ArtistDetailResponse>, ProblemHttpResult, ValidationProblem>
                > (string sqid, string? sort, ISender sender) =>
                {
                    ErrorOr<ArtistDetailResponse> result = await sender.Send(
                        new GetArtistByIdQuery(sqid, sort ?? nameof(ArtistAlbumSortBy.NewestFirst))
                    );

                    return result.Match<
                        Results<Ok<ArtistDetailResponse>, ProblemHttpResult, ValidationProblem>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.ProblemWithValidationProblem<Ok<ArtistDetailResponse>>()
                    );
                }
            )
            .WithName("GetArtistById")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
