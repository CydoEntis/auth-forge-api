using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using FluentValidation;
using Mediator;

namespace AuthForge.Application.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TMessage>(message);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var errors = failures
                .Select(f => new Error(f.PropertyName, f.ErrorMessage))
                .ToArray();

            return CreateValidationResult<TResponse>(errors);
        }

        return await next(message, cancellationToken);
    }

    private static TResponse CreateValidationResult<T>(Error[] errors)
        where T : Result
    {
        if (typeof(T) == typeof(Result))
        {
            return (ValidationResult.WithErrors(errors) as TResponse)!;
        }

        var validationResultType = typeof(ValidationResult<>)
            .MakeGenericType(typeof(T).GenericTypeArguments[0]);

        var validationResult = Activator.CreateInstance(
            validationResultType,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            args: new object[] { errors },
            culture: null);

        return (validationResult as TResponse)!;
    }
}