using ErrorOr;
using MediatR;

namespace HalcyonRecords.Api.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid PublicId) : IRequest<ErrorOr<CurrentUserResponse>>;
