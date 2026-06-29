using MediatR;
using Microsoft.Extensions.Logging;

namespace Api.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Iniciando request {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);

            _logger.LogInformation("Request {RequestName} completado", requestName);

            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Request {RequestName} fallo", requestName);
            throw;
        }
    }
}
