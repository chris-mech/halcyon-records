using ErrorOr;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HalcyonRecords.Api.Common.Behaviours;
using MediatR;
using NSubstitute;

namespace HalcyonRecords.Api.UnitTests.Common.Behaviours;

public class ValidationBehaviourTests
{
    public sealed record DummyRequest : IRequest<ErrorOr<string>>;

    [Fact]
    public async Task Handle_CallsNext_WhenNoValidatorIsRegistered()
    {
        var behaviour = new ValidationBehaviour<DummyRequest, string>(validator: null);
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();
        next.Invoke(Arg.Any<CancellationToken>()).Returns("ok");

        var result = await behaviour.Handle(new DummyRequest(), next, CancellationToken.None);

        result.Value.Should().Be("ok");
        await next.Received(1).Invoke(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallsNext_WhenValidationSucceeds()
    {
        var validator = Substitute.For<IValidator<DummyRequest>>();
        validator
            .ValidateAsync(Arg.Any<DummyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behaviour = new ValidationBehaviour<DummyRequest, string>(validator);
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();
        next.Invoke(Arg.Any<CancellationToken>()).Returns("ok");

        var result = await behaviour.Handle(new DummyRequest(), next, CancellationToken.None);

        result.Value.Should().Be("ok");
        await next.Received(1).Invoke(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsValidationErrors_WithoutCallingNext_WhenValidationFails()
    {
        var failure = new ValidationFailure("SomeProperty", "Some error");
        var validator = Substitute.For<IValidator<DummyRequest>>();
        validator
            .ValidateAsync(Arg.Any<DummyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([failure]));

        var behaviour = new ValidationBehaviour<DummyRequest, string>(validator);
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();

        var result = await behaviour.Handle(new DummyRequest(), next, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("SomeProperty");
        await next.DidNotReceive().Invoke(Arg.Any<CancellationToken>());
    }
}
