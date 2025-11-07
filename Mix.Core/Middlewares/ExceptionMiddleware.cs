using Microsoft.Extensions.Logging;
using Mix.Core.Contexts;

namespace Mix.Core.Middlewares;

public class ExceptionMiddleware
{
    private readonly Func<PrintContext, Task> _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    public ExceptionMiddleware(
        Func<PrintContext, Task> next,
        ILogger<ExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(PrintContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 PrintContext 时发生异常: {Message}", ex);
        }
    }
}
