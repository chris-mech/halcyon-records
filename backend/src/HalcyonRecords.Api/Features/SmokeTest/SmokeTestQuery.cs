using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.SmokeTest;

public sealed record SmokeTestQuery(bool ShouldFail) : IRequest<ErrorOr<SmokeTestResponse>>;

public sealed record SmokeTestResponse(string Message);
