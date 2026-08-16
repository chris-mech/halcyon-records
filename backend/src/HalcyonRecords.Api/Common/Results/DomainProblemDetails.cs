using Microsoft.AspNetCore.Mvc;

namespace HalcyonRecords.Api.Common.Results;

public sealed class DomainProblemDetails : ProblemDetails
{
    public required string Code { get; init; }
}
