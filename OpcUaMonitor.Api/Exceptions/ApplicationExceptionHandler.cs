using Microsoft.AspNetCore.Diagnostics;

namespace OpcUaMonitor.Api.Exceptions;

public class ApplicationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not ApplicationException)
            return ValueTask.FromResult(false);

        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = "Application Error",
                Detail = exception.Message,
                Status = StatusCodes.Status500InternalServerError,
            },
        };

        return problemDetailsService.TryWriteAsync(problemDetailsContext);
    }
}
