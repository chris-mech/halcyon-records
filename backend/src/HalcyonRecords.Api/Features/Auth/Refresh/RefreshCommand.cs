using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Auth.Refresh;

public sealed record RefreshCommand(string RefreshToken) : IRequest<ErrorOr<RefreshResponse>>;
