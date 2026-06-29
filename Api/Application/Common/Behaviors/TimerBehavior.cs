using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Api.Application.Common.Behaviors;

public sealed class TimerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMilliseconds = 500;

    private readonly ILogger<TimerBehavior<TRequest, TResponse>> _logger;

    public TimerBehavior(ILogger<TimerBehavior<TRequest, TResponse>> logger)
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
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds >= SlowRequestThresholdMilliseconds)
            {
                _logger.LogWarning(
                    "Request {RequestName} tomo {ElapsedMilliseconds} ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds
                );
            }
            else
            {
                _logger.LogInformation(
                    "Request {RequestName} tomo {ElapsedMilliseconds} ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds
                );
            }
        }
    }
}
