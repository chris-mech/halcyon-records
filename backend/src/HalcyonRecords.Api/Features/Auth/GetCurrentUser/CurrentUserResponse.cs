namespace HalcyonRecords.Api.Features.Auth.GetCurrentUser;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset RegisteredAt
);
