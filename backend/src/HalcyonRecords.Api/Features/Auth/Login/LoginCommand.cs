using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<ErrorOr<LoginResponse>>;
