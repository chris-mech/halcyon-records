using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetAlbumById;

public sealed class GetAlbumByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums/{sqid}/{titleSlug?}",
                async Task<
                    Results<
                        Ok<AlbumDetailResponse>,
                        RedirectHttpResult,
                        ProblemHttpResult,
                        ValidationProblem
                    >
                > (
                    string sqid,
                    string? titleSlug,
                    LinkGenerator linkGenerator,
                    HttpContext httpContext,
                    ISender sender
                ) =>
                {
                    ErrorOr<AlbumDetailResponse> result = await sender.Send(
                        new GetAlbumByIdQuery(sqid)
                    );

                    return result.Match<
                        Results<
                            Ok<AlbumDetailResponse>,
                            RedirectHttpResult,
                            ProblemHttpResult,
                            ValidationProblem
                        >
                    >(
                        response =>
                            titleSlug == response.TitleSlug
                                ? TypedResults.Ok(response)
                                : TypedResults.Redirect(
                                    linkGenerator.GetPathByName(
                                        httpContext,
                                        "GetAlbumById",
                                        new { sqid = response.Sqid, titleSlug = response.TitleSlug }
                                    )
                                        ?? throw new InvalidOperationException(
                                            "Could not generate the canonical album URL."
                                        ),
                                    permanent: true
                                ),
                        errors => errors.Problem<Ok<AlbumDetailResponse>, RedirectHttpResult>()
                    );
                }
            )
            .WithName("GetAlbumById");
    }
}
