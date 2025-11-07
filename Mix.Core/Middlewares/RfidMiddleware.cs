

using Microsoft.Extensions.Logging;
using Mix.Core.Contexts;

namespace Mix.Core.Middlewares;

public class RfidMiddleware
{
    private readonly Func<PrintContext, Task> _next;
    private readonly ILogger<LogMiddleware> _logger;

    public RfidMiddleware(Func<PrintContext, Task> next, ILogger<LogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(PrintContext context)
    {
        context.Container = new Entity.Container("~~");
        await _next(context);
    }
}
