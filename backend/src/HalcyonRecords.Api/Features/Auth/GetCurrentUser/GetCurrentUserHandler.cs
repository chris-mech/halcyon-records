using ErrorOr;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;

namespace HalcyonRecords.Api.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetCurrentUserQuery, ErrorOr<CurrentUserResponse>>
{
    public async Task<ErrorOr<CurrentUserResponse>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Users.FindAsync([query.UserId], cancellationToken);
        if (user is null)
        {
            return DomainErrors.Auth.UserNotFound();
        }

        return new CurrentUserResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.RegisteredAt
        );
    }
}
