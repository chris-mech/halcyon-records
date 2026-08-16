using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserQuery(int UserId) : IRequest<ErrorOr<CurrentUserResponse>>;
