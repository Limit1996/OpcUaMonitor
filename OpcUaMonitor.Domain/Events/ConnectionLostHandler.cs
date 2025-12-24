using MediatR;
using Microsoft.Extensions.Logging;
using OpcUaMonitor.Domain.Manager;

namespace OpcUaMonitor.Domain.Events;

public class ConnectionLostHandler :INotificationHandler<ConnectionLostEvent>
{
    
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly OpcUaManager _opcUaManager;
    
    public ConnectionLostHandler(
        IMediator mediator,
        ILogger<ConnectionLostHandler> logger,
        ILoggerFactory loggerFactory,
        OpcUaManager opcUaManager)
    {
        _mediator = mediator;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _opcUaManager = opcUaManager;
    }
    
    
    public async Task Handle(ConnectionLostEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("通道 {ChannelUrl} 连接丢失，正在尝试重连...", notification.Channel.Url);
        var opcUaProviders = _opcUaManager.OpcUaProviders;
        var channel = notification.Channel;
        if (opcUaProviders.TryGetValue(channel, out var value))
        {
            await value.DisposeAsync();
            opcUaProviders.Remove(channel);
        }

        // 重试重连（带延迟和重试次数限制）
        for (var i = 1; i <= 3; i++)
        {
            _logger.LogInformation("正在尝试第 {Attempt} 次重连通道 {ChannelUrl}...", i, channel.Url);
            await Task.Delay(TimeSpan.FromSeconds(i * Math.Pow(20, i - 1)), cancellationToken);
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

            opcUaProviders.Add(channel, provider);
            _logger.LogInformation("通道 {ChannelUrl} 重连成功。", channel.Url);
            break;
        }
    }
}