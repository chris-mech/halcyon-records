using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<ErrorOr<Success>>;
