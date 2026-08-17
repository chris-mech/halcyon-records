using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<ErrorOr<Success>>;
