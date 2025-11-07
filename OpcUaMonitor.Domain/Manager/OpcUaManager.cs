using MediatR;
using Microsoft.Extensions.Logging;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Manager;

public class OpcUaManager : IAsyncDisposable
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
}