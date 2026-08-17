namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);
