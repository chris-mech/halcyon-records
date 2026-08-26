using System.ComponentModel;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Albums.GetAlbums;

public sealed class GetAlbumsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/albums",
                async Task<
                    Results<
                        Ok<PagedResult<AlbumSummaryResponse>>,
                        ProblemHttpResult,
                        ValidationProblem
                    >
                > (
                    [Description("The page number to return, starting at 1. Defaults to 1.")]
                        int? page,
                    [Description("The number of items per page, from 1 to 50. Defaults to 12.")]
                        int? pageSize,
                    [Description("Only return albums flagged as new arrivals.")] bool? isNew,
                    [Description(
                        "Only return albums currently discounted from their original price."
                    )]
                        bool? isOnSale,
                    [Description("Only return albums flagged as staff picks.")] bool? isStaffPick,
                    [Description("Only return albums with at least one unit in stock.")]
                        bool? inStock,
                    [Description(
                        "Genre slugs to filter by. An album matching any of the given genres is included."
                    )]
                        string[]? genres,
                    [Description("Only return albums released in or after this year.")]
                        int? startYear,
                    [Description("Only return albums released in or before this year.")]
                        int? endYear,
                    [Description("The sort order to apply. Defaults to NewestFirst.")] string? sort,
                    ISender sender
                ) =>
                {
                    ErrorOr<PagedResult<AlbumSummaryResponse>> result = await sender.Send(
                        new GetAlbumsQuery(
                            page ?? 1,
                            pageSize ?? 12,
                            isNew ?? false,
                            isOnSale ?? false,
                            isStaffPick ?? false,
                            inStock ?? false,
                            genres,
                            startYear,
                            endYear,
                            sort ?? nameof(AlbumSortBy.NewestFirst)
                        )
                    );

                    return result.Match<
                        Results<
                            Ok<PagedResult<AlbumSummaryResponse>>,
                            ProblemHttpResult,
                            ValidationProblem
                        >
                    >(
                        response => TypedResults.Ok(response),
                        errors =>
                            errors.ProblemWithValidationProblem<
                                Ok<PagedResult<AlbumSummaryResponse>>
                            >()
                    );
                }
            )
            .WithName("GetAlbums")
            .WithTags("Albums")
            .WithSummary("List albums with filtering, sorting, and pagination.")
            .WithDescription(
                "Boolean filters and the year range combine with AND; the genres filter combines "
                    + "with OR, matching an album that has any of the given genre slugs."
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    operation.SetParameterExample("page", JsonValue.Create(1));
                    operation.SetParameterExample("pageSize", JsonValue.Create(12));
                    operation.SetParameterExample("isNew", JsonValue.Create(true));
                    operation.SetParameterExample("isOnSale", JsonValue.Create(true));
                    operation.SetParameterExample("isStaffPick", JsonValue.Create(true));
                    operation.SetParameterExample("inStock", JsonValue.Create(true));
                    operation.SetParameterExample(
                        "genres",
                        JsonNode.Parse("""["shoegaze", "dream-pop"]""")!
                    );
                    operation.SetParameterExample("startYear", JsonValue.Create(1990));
                    operation.SetParameterExample("endYear", JsonValue.Create(1999));
                    operation.SetParameterExample(
                        "sort",
                        JsonValue.Create(nameof(AlbumSortBy.NewestFirst))
                    );
                    return Task.CompletedTask;
                }
            );
    }
}
