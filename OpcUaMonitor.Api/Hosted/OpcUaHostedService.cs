using OpcUaMonitor.Domain.Manager;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Api.Hosted;

public class OpcUaHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OpcUaHostedService> _logger;
    private readonly OpcUaManager _opcUaManager;

    public OpcUaHostedService(
        IServiceScopeFactory scopeFactory,
        OpcUaManager opcUaManager,
        ILogger<OpcUaHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _opcUaManager = opcUaManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OpcUaHostedService is starting.");

        // 启动时初始化连接
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUaRepository>();
        
        var channels = await repository.GetChannelsAsync(stoppingToken);
        var events = await repository.GetEventsAsync(stoppingToken);
        
        await _opcUaManager.StartMonitoringAsync(channels.ToArray(), events.ToArray(), stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OpcUaHostedService is stopping gracefully.");

        await _opcUaManager.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}