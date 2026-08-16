using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HalcyonRecords.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HalcyonRecords.Api.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JwtOptions _options = options.Value;

    public (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(User user)
    {
        var expiresAt = timeProvider.GetUtcNow().AddMinutes(_options.AccessTokenLifetimeMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
            ]),
            Expires = expiresAt.UtcDateTime,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
                SecurityAlgorithms.HmacSha256
            ),
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);
        return (accessToken, expiresAt);
    }

    public (string RawToken, string TokenHash, DateTimeOffset ExpiresAt) GenerateRefreshToken()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiresAt = timeProvider.GetUtcNow().AddDays(_options.RefreshTokenLifetimeDays);
        return (rawToken, HashToken(rawToken), expiresAt);
    }

    public string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
