using ErrorOr;
using HalcyonRecords.Api.Infrastructure.Sql;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetCurrentUserQuery, ErrorOr<CurrentUserResponse>>
{
    public async Task<ErrorOr<CurrentUserResponse>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.PublicId == query.PublicId,
            cancellationToken
        );
        if (user is null)
        {
            return DomainErrors.Auth.UserNotFound();
        }

        return new CurrentUserResponse(
            user.PublicId,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.RegisteredAt
        );
    }
}
