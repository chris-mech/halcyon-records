namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed record RegisterResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
