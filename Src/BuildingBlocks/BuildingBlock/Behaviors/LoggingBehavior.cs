namespace BuildingBlock.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    (ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[START] Handle request={Request} - Response={Response} - RequestDate={RequestData}",
             typeof(TRequest).Name,
             typeof(TResponse).Name,
             request );

        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next();

            timer.Stop();

            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds} ms → {@Response}",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds,
                response);

            if (timer.ElapsedMilliseconds > 3000)// if the request is greater than 3 seconds, then log the warnings
            {
                logger.LogWarning(
                    "[PERFORMANCE] {RequestName} took {ElapsedMilliseconds} ms",
                    typeof(TResponse).Name,
                    timer.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {

            timer.Stop();

            logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds} ms with payload {@Request}",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds,
                request);

            throw;
        }
    }
}
