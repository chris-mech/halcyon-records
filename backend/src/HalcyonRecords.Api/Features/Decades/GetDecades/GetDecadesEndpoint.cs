using ErrorOr;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HalcyonRecords.Api.Features.Decades.GetDecades;

public sealed class GetDecadesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/decades",
                async Task<Results<Ok<IReadOnlyList<DecadeListItemResponse>>, ProblemHttpResult>> (
                    ISender sender
                ) =>
                {
                    ErrorOr<IReadOnlyList<DecadeListItemResponse>> result = await sender.Send(
                        new GetDecadesQuery()
                    );

                    return result.Match<
                        Results<Ok<IReadOnlyList<DecadeListItemResponse>>, ProblemHttpResult>
                    >(
                        response => TypedResults.Ok(response),
                        errors => errors.Problem<Ok<IReadOnlyList<DecadeListItemResponse>>>()
                    );
                }
            )
            .WithName("GetDecades")
            .WithTags("Decades")
            .WithSummary("List all decades with each decade's album count.");
    }
}
