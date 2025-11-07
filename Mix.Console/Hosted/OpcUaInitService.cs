using App.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Mix.Core.Contexts;
using Mix.Core.Middlewares;
using OpcUaMonitor.Domain.Manager;

namespace Mix.Console.Hosted;

internal class OpcUaInitService : IHostedService
{
    private readonly OpcUaManager _opcUaManager;
    private readonly IConfiguration _configuration;

    public OpcUaInitService(
        OpcUaManager opcUaManager,
        IConfiguration configuration
    )
    {
        _opcUaManager = opcUaManager;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var opcUaUrl = _configuration.GetValue<string>("Device:Url");

        var channel = OpcUaMonitor.Domain.Ua.Channel.Create(opcUaUrl!, _configuration["Print:Channel"]!);

        await _opcUaManager.StartMonitoringAsync([channel], [], cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _opcUaManager.DisposeAsync();
    }
}


internal class PrintInitService : IHostedService
{
    private readonly IAppBuilder<PrintContext> _appBuilder;
    private readonly IServer<PrintContext> _server;
    public PrintInitService(
        IAppBuilder<PrintContext> appBuilder,
        IServer<PrintContext> server
    )
    {
        _appBuilder = appBuilder;
        _server = server;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _appBuilder.UseMiddleware<LogMiddleware, PrintContext>();
        _appBuilder.UseMiddleware<ExceptionMiddleware, PrintContext>();
        _appBuilder.UseMiddleware<RfidMiddleware, PrintContext>();
        _appBuilder.UseMiddleware<PersistenceMiddleware, PrintContext>();
        //添加所需的中间件

        var app = _appBuilder.Build();
        await _server.StartAsync(app);

    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _server.DisposeAsync();
    }
}