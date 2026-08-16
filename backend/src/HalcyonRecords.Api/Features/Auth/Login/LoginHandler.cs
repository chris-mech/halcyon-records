using ErrorOr;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed class LoginHandler(
    UserManager<User> userManager,
    JwtTokenService jwtTokenService,
    ApplicationDbContext dbContext,
    TimeProvider timeProvider
) : IRequestHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    public async Task<ErrorOr<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            return DomainErrors.Auth.InvalidCredentials();
        }

        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var (rawRefreshToken, tokenHash, refreshExpiresAt) = jwtTokenService.GenerateRefreshToken();

        dbContext.RefreshTokens.Add(
            new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                CreatedAt = timeProvider.GetUtcNow(),
                ExpiresAt = refreshExpiresAt,
            }
        );
        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, rawRefreshToken, expiresAt);
    }
}
