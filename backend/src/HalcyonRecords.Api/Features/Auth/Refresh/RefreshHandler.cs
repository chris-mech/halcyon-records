using ErrorOr;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Auth.Refresh;

public sealed class RefreshHandler(
    JwtTokenService jwtTokenService,
    ApplicationDbContext dbContext,
    TimeProvider timeProvider
) : IRequestHandler<RefreshCommand, ErrorOr<RefreshResponse>>
{
    public async Task<ErrorOr<RefreshResponse>> Handle(
        RefreshCommand command,
        CancellationToken cancellationToken
    )
    {
        var tokenHash = jwtTokenService.HashToken(command.RefreshToken);
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.TokenHash == tokenHash,
            cancellationToken
        );

        if (token is null)
        {
            return DomainErrors.Auth.InvalidRefreshToken();
        }

        var now = timeProvider.GetUtcNow();

        if (token.RevokedAt is not null)
        {
            await RevokeDescendantsAsync(token, now, cancellationToken);
            await dbContext.SaveChangesAsync(CancellationToken.None);
            return DomainErrors.Auth.InvalidRefreshToken();
        }

        if (token.ExpiresAt <= now)
        {
            return DomainErrors.Auth.InvalidRefreshToken();
        }

        var user = await dbContext.Users.FindAsync([token.UserId], cancellationToken);
        if (user is null)
        {
            return DomainErrors.Auth.InvalidRefreshToken();
        }

        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var (rawRefreshToken, newTokenHash, refreshExpiresAt) =
            jwtTokenService.GenerateRefreshToken();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        var newToken = new RefreshToken
        {
            TokenHash = newTokenHash,
            UserId = user.Id,
            CreatedAt = now,
            ExpiresAt = refreshExpiresAt,
        };
        dbContext.RefreshTokens.Add(newToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        token.RevokedAt = now;
        token.ReplacedByTokenId = newToken.Id;
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(CancellationToken.None);

        return new RefreshResponse(accessToken, rawRefreshToken, expiresAt);
    }

    private async Task RevokeDescendantsAsync(
        RefreshToken token,
        DateTimeOffset now,
        CancellationToken cancellationToken
    )
    {
        var nextId = token.ReplacedByTokenId;
        while (nextId is { } id)
        {
            var next = await dbContext.RefreshTokens.FindAsync([id], cancellationToken);
            if (next is null)
            {
                break;
            }

            next.RevokedAt = now;
            nextId = next.ReplacedByTokenId;
        }
    }
}
