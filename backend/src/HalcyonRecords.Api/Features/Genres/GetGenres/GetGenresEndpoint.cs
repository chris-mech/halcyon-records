using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Genres.GetGenres;

public sealed class GetGenresEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/genres",
                async Task<Results<Ok<IReadOnlyList<GenreListItemResponse>>, ProblemHttpResult>> (
                    ISender sender
                ) =>
                {
                    ErrorOr<IReadOnlyList<GenreListItemResponse>> result = await sender.Send(
                        new GetGenresQuery()
                    );

                    return result.Match<
                        Results<Ok<IReadOnlyList<GenreListItemResponse>>, ProblemHttpResult>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<IReadOnlyList<GenreListItemResponse>>>()
                    );
                }
            )
            .WithName("GetGenres")
            .WithTags("Genres")
            .WithSummary("List all genres with each genre's album count.");
    }
}
