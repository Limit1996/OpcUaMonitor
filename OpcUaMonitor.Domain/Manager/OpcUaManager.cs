using MediatR;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Manager;

public class OpcUaManager : IAsyncDisposable
{
    private readonly IMediator _mediator;
    private Dictionary<Channel, IOpcUaProvider> _opcUaProviders = new();

    public Dictionary<Channel, IOpcUaProvider> OpcUaProviders => _opcUaProviders;

    public OpcUaManager(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task StartMonitoringAsync(
        Channel[] channels,
        Event[] events,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(events);

        foreach (var channel in channels)
        {
            var provider = new OpcUaProvider(_mediator);
            var isConnected = await provider.ConnectAsync(
                config =>
                {
                    config.ApplicationName = "OpcUaMonitor";
                },
                channel,
                cancellationToken
            );
            if (!isConnected)
                continue;
            await provider.RegisterDataChangeHandler(
                events.Where(e => e.Tag.Name.Contains(channel.Name)).ToArray()
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
