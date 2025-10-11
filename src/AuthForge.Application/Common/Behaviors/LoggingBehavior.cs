using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Common.Behaviors;

public sealed class LoggingBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ILogger<LoggingBehavior<TMessage, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;

        _logger.LogInformation(
            "Handling {MessageName}",
            messageName);

        var response = await next(message, cancellationToken);

        _logger.LogInformation(
            "Handled {MessageName}",
            messageName);

        return response;
    }
}