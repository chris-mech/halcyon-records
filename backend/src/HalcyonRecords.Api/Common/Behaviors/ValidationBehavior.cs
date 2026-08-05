using ErrorOr;
using FluentValidation;
using MediatR;

namespace HalcyonRecords.Api.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResult>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, ErrorOr<TResult>>
    where TRequest : IRequest<ErrorOr<TResult>>
{
    public async Task<ErrorOr<TResult>> Handle(
        TRequest request,
        RequestHandlerDelegate<ErrorOr<TResult>> next,
        CancellationToken cancellationToken
    )
    {
        if (validator is null)
        {
            return await next(cancellationToken);
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next(cancellationToken);
        }

        return validationResult.Errors.ConvertAll(failure =>
            Error.Validation(failure.PropertyName, failure.ErrorMessage)
        );
    }
}
