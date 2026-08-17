namespace HalcyonRecords.Api.Features.Auth.Refresh;

public sealed record RefreshResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
