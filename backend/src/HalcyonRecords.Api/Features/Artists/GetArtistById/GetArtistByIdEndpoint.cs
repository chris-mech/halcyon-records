using System.ComponentModel;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
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
                > (
                    [Description("The artist's sqid, as returned by other artist endpoints.")]
                        string sqid,
                    [Description(
                        "The sort order to apply to the artist's discography. Defaults to NewestFirst."
                    )]
                        string? sort,
                    ISender sender
                ) =>
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
            .WithTags("Artists")
            .WithSummary("Get a single artist's detail and discography by sqid.")
            .Produces<DomainProblemDetails>(
                StatusCodes.Status404NotFound,
                "application/problem+json"
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("sqid", JsonValue.Create("9pXqL2"));
                    operation.SetParameterExample(
                        "sort",
                        JsonValue.Create(nameof(ArtistAlbumSortBy.NewestFirst))
                    );
                    return Task.CompletedTask;
                }
            );
    }
}
