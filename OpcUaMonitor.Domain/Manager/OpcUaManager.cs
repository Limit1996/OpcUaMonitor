using MediatR;
using Microsoft.Extensions.Logging;
using OpcUaMonitor.Domain.Events;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Manager;

public class OpcUaManager : IAsyncDisposable, INotificationHandler<ConnectionLostEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<Channel, IOpcUaProvider> _opcUaProviders = new();

    public Dictionary<Channel, IOpcUaProvider> OpcUaProviders => _opcUaProviders;

    public OpcUaManager(IMediator mediator, ILogger<OpcUaManager> logger, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task StartMonitoringAsync(
        Channel[] channels,
        Event[] events,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(events);

        var providerLogger = _loggerFactory.CreateLogger<OpcUaProvider>();
        foreach (var channel in channels)
        {
            var provider = new OpcUaProvider(_mediator, providerLogger);
            var isConnected = await provider.ConnectAsync(
                config => { config.ApplicationName = "OpcUaMonitor"; },
                channel,
                cancellationToken
            );
            if (!isConnected)
                continue;
            await provider.RegisterDataChangeHandler(
                [.. events.Where(e => e.ChannelId == channel.Id)]
            );
            _opcUaProviders.Add(channel, provider);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var provider in _opcUaProviders.Values)
        {
            await provider.DisposeAsync();
        }
    }

    public async Task Handle(ConnectionLostEvent notification, CancellationToken cancellationToken)
    {
        var channel = notification.Channel;
        if (!_opcUaProviders.TryGetValue(channel, out var value))
            return;

        await value.DisposeAsync();
        _opcUaProviders.Remove(channel);

        // 重试重连（带延迟和重试次数限制）
        for (var i = 1; i <= 3; i++)
        {
            _logger.LogInformation("正在尝试第 {Attempt} 次重连通道 {ChannelUrl}...", i, channel.Url);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            var providerLogger = _loggerFactory.CreateLogger<OpcUaProvider>();
            var provider = new OpcUaProvider(_mediator, providerLogger);
            if (!await provider.ConnectAsync(_ => { }, channel, cancellationToken))
            {
                _logger.LogError("第 {Attempt} 次重连通道 {ChannelUrl} 失败。", i, channel.Url);
                if (i == 3)
                {
                    _logger.LogError("通道 {ChannelUrl} 重连失败，已达最大重试次数(3)。", channel.Url);
                }
                continue;
            }

            _opcUaProviders.Add(channel, provider);
            _logger.LogInformation("通道 {ChannelUrl} 重连成功。", channel.Url);
            break;
        }
    }
}