using ErrorOr;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed class RegisterHandler(
    UserManager<User> userManager,
    JwtTokenService jwtTokenService,
    ApplicationDbContext dbContext,
    TimeProvider timeProvider
) : IRequestHandler<RegisterCommand, ErrorOr<RegisterResponse>>
{
    public async Task<ErrorOr<RegisterResponse>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = new User
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            RegisteredAt = timeProvider.GetUtcNow(),
        };

        var createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            if (
                createResult.Errors.Any(error =>
                    error.Code is "DuplicateEmail" or "DuplicateUserName"
                )
            )
            {
                return DomainErrors.Auth.EmailAlreadyRegistered(command.Email);
            }

            return createResult
                .Errors.Select(error =>
                    Error.Validation(code: $"Auth.{error.Code}", description: error.Description)
                )
                .ToList();
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

        return new RegisterResponse(accessToken, rawRefreshToken, expiresAt);
    }
}
