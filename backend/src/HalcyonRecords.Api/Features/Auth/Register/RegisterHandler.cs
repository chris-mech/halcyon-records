using ErrorOr;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed class RegisterHandler(UserManager<User> userManager, TimeProvider timeProvider)
    : IRequestHandler<RegisterCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
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
            LastActiveAt = timeProvider.GetUtcNow(),
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

        return Result.Success;
    }
}
