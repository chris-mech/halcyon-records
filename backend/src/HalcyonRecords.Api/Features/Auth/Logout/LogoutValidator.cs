using FluentValidation;

namespace HalcyonRecords.Api.Features.Auth.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
