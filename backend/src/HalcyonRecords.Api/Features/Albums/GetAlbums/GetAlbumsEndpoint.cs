using System.ComponentModel;
using System.Text.Json.Nodes;
using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

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
                    [Description("The page number to return.")] int? page,
                    [Description("The number of items per page.")] int? pageSize,
                    [Description("Only return albums flagged as new arrivals.")]
                    [DefaultValue(false)]
                        bool? isNew,
                    [Description(
                        "Only return albums currently discounted from their original price."
                    )]
                    [DefaultValue(false)]
                        bool? isOnSale,
                    [Description("Only return albums flagged as staff picks.")]
                    [DefaultValue(false)]
                        bool? isStaffPick,
                    [Description("Only return albums with at least one unit in stock.")]
                    [DefaultValue(false)]
                        bool? inStock,
                    [Description(
                        "Genre slugs to filter by. An album matching any of the given genres is included."
                    )]
                        string[]? genres,
                    [Description("Only return albums released in or after this year.")]
                        int? startYear,
                    [Description("Only return albums released in or before this year.")]
                        int? endYear,
                    [Description("The sort order to apply.")]
                    [DefaultValue(nameof(AlbumSortBy.NewestFirst))]
                        string? sort,
                    IOptions<AlbumsPaginationOptions> paginationOptions,
                    ISender sender
                ) =>
                {
                    var pagination = paginationOptions.Value;

                    ErrorOr<PagedResult<AlbumSummaryResponse>> result = await sender.Send(
                        new GetAlbumsQuery(
                            page ?? pagination.MinPage,
                            pageSize ?? pagination.DefaultPageSize,
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
                    + "with OR, matching an album that has any of the given genre slugs. When both "
                    + "startYear and endYear are given, endYear must be greater than or equal to "
                    + "startYear."
            )
            .AddOpenApiOperationTransformer(
                (operation, context, cancellationToken) =>
                {
                    var pagination = context
                        .ApplicationServices.GetRequiredService<IOptions<AlbumsPaginationOptions>>()
                        .Value;

                    operation.SetParameterExample("page", JsonValue.Create(1));
                    operation.SetParameterMinimum("page", pagination.MinPage);
                    operation.SetParameterDefault("page", JsonValue.Create(pagination.MinPage));
                    operation.SetParameterExample("pageSize", JsonValue.Create(12));
                    operation.SetParameterRange(
                        "pageSize",
                        pagination.MinPageSize,
                        pagination.MaxPageSize
                    );
                    operation.SetParameterDefault(
                        "pageSize",
                        JsonValue.Create(pagination.DefaultPageSize)
                    );
                    operation.SetParameterExample("isNew", JsonValue.Create(true));
                    operation.SetParameterExample("isOnSale", JsonValue.Create(true));
                    operation.SetParameterExample("isStaffPick", JsonValue.Create(true));
                    operation.SetParameterExample("inStock", JsonValue.Create(true));
                    operation.SetParameterExample(
                        "genres",
                        JsonNode.Parse("""["shoegaze", "dream-pop"]""")!
                    );
                    operation.SetParameterArrayConstraints(
                        "genres",
                        maxItems: 50,
                        itemMaxLength: 150
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
