using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.SmokeTest;

public sealed class SmokeTestQueryHandler
    : IRequestHandler<SmokeTestQuery, ErrorOr<SmokeTestResponse>>
{
    public Task<ErrorOr<SmokeTestResponse>> Handle(
        SmokeTestQuery request,
        CancellationToken cancellationToken
    )
    {
        if (request.ShouldFail)
        {
            return Task.FromResult<ErrorOr<SmokeTestResponse>>(
                Error.NotFound(
                    code: "SmokeTest.Failed",
                    description: "Smoke test was asked to fail."
                )
            );
        }

        return Task.FromResult<ErrorOr<SmokeTestResponse>>(
            new SmokeTestResponse("Pipeline is wired correctly.")
        );
    }
}
