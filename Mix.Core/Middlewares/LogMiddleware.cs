
using Microsoft.Extensions.Logging;
using Mix.Core.Contexts;

namespace Mix.Core.Middlewares;

public class LogMiddleware
{
    private readonly Func<PrintContext, Task> _next;
    private readonly ILogger<LogMiddleware> _logger;

    public LogMiddleware(Func<PrintContext, Task> next, ILogger<LogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(PrintContext context)
    {
        _logger.LogInformation("LogMiddleware: 正在处理 PrintContext...");
        await _next(context);
        _logger.LogInformation("LogMiddleware: 处理完成 PrintContext.");
    }
}
