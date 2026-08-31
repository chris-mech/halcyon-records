using ErrorOr;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Api.Infrastructure.Sql;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Auth.Logout;

public sealed class LogoutHandler(
    JwtTokenService jwtTokenService,
    ApplicationDbContext dbContext,
    TimeProvider timeProvider
) : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken
    )
    {
        var tokenHash = jwtTokenService.HashToken(command.RefreshToken);
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.TokenHash == tokenHash,
            cancellationToken
        );

        if (token is not null && token.RevokedAt is null)
        {
            token.RevokedAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        return Result.Success;
    }
}
