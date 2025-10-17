using System.Diagnostics.CodeAnalysis;
using OpcUaMonitor.Domain.Abstractions;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Sys;

public sealed class Device : Entity
{
    public string Code { get; set; }
    public string Name { get; set; }

    public string Manufacturer { get; set; }

    [StringSyntax(StringSyntaxAttribute.Uri)]
    public string IpAddress { get; set; }
    public string Specification { get; set; }
    
    public Guid ProcessId { get; }

    private readonly List<Channel> _channels = [];
    public IReadOnlyList<Channel> Channels => _channels;

    public void AddChannel(Channel channel)
    {
        if (_channels.Any(c => c.Id == channel.Id))
            throw new ArgumentException($"Channel with name {channel.Name} already exists");
        _channels.Add(channel);
    }

    public void RemoveChannel(Channel channel)
    {
        if (_channels.All(c => c.Id != channel.Id))
            throw new ArgumentException($"Channel with id {channel.Id} does not exist");
        _channels.Remove(channel);
    }

    private Device() { }

    private Device(
        Guid id,
        string code,
        string name,
        string manufacturer,
        string ipAddress,
        string specification
    )
        : base(id)
    {
        Code = code;
        Name = name;
        Manufacturer = manufacturer;
        IpAddress = ipAddress;
        Specification = specification;
    }

    public static Device Create(
        string code,
        string name,
        string manufacturer,
        [StringSyntax(StringSyntaxAttribute.Uri)] string ipAddress,
        string specification
    ) => new(Guid.NewGuid(), code, name, manufacturer, ipAddress, specification);
}
