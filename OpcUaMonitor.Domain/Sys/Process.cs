using OpcUaMonitor.Domain.Abstractions;
using OpcUaMonitor.Domain.Shared;

namespace OpcUaMonitor.Domain.Sys;

public class Process : Entity
{
    public string Name { get; set; }
    public Area Area { get; set; }
    
    private readonly List<Device> _devices = [];
    public IReadOnlyList<Device> Devices => _devices;
    public string Description { get; set; }
    
    public void AddDevice(Device device)
    {
        if (_devices.Any(d => d.Id == device.Id))
            throw new ArgumentException($"Device with id {device.Id} already exists");
        _devices.Add(device);
    }
    public void RemoveDevice(Device device)
    {
        if (_devices.All(d => d.Id != device.Id))
            throw new ArgumentException($"Device with id {device.Id} does not exist");
        _devices.Remove(device);
    }

    private Process(string name, string description)
        : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
    }

    private Process() { }

    public static Process Create(string name, string description)
    {
        return string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Process name cannot be null or empty.")
            : new Process(name, description);
    }
}
