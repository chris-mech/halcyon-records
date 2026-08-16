using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Genres.GetGenreBySlug;

public sealed class GetGenreBySlugEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/genres/{slug}",
                async Task<Results<Ok<GenreDetailResponse>, ProblemHttpResult>> (
                    string slug,
                    ISender sender
                ) =>
                {
                    ErrorOr<GenreDetailResponse> result = await sender.Send(
                        new GetGenreBySlugQuery(slug)
                    );

                    return result.Match<Results<Ok<GenreDetailResponse>, ProblemHttpResult>>(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<GenreDetailResponse>>()
                    );
                }
            )
            .WithName("GetGenreBySlug")
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            );
    }
}
