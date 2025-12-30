using Opc.Ua;
using Opc.Ua.Client;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Manager;

public interface IOpcUaProvider : IAsyncDisposable
{
    bool IsConnected { get; }

    Session? GetSession();

    Task<bool> ConnectAsync(
        Action<ApplicationConfiguration> config,
        Channel channel,
        CancellationToken cancellationToken = default
    );

    ValueTask DisconnectAsync();

    Task WriteAsync<T>(string tag, T value, CancellationToken cancellationToken = default);
    Task WriteMultipleAsync<T>(
        Dictionary<string, T> tagValuePairs,
        CancellationToken cancellationToken = default
    );

    Task<T> ReadAsync<T>(string tag, CancellationToken cancellationToken = default);

    Task<Dictionary<string, object>> ReadMultipleAsync(
        string[] tags,
        CancellationToken cancellationToken = default
    );

    internal Task RegisterDataChangeHandler(Event @event);
    internal Task UnregisterDataChangeHandler(Event @event);
}
