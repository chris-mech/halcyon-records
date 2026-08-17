namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
