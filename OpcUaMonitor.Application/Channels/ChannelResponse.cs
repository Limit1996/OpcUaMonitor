namespace OpcUaMonitor.Application.Channels;

public sealed class ChannelResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string OpcUrl { get; init; } = null!;
}