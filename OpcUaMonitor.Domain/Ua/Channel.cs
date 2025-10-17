using Opc.Ua;
using OpcUaMonitor.Domain.Abstractions;
using OpcUaMonitor.Domain.Shared;

namespace OpcUaMonitor.Domain.Ua;

public class Channel : Entity
{
    public string Url { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    

    private readonly List<Device> _devices = [];
    public IReadOnlyList<Device> Devices => _devices;

    public void AddDevice(Device device)
    {
        if (_devices.Any(d => d.Name == device.Name))
            throw new ArgumentException($"Device with name {device.Name} already exists");
        _devices.Add(device);
    }

    public void RemoveDevice(Device device)
    {
        if (_devices.All(d => d.Id != device.Id))
            throw new ArgumentException($"Device with id {device.Id} does not exist");
        _devices.Remove(device);
    }

    private Channel() { }

    private Channel(Guid id, string url, string name)
        : base(id)
    {
        Url = url;
        Name = name;
    }

    public static Channel Create(string url, string name) => new(Guid.NewGuid(), url, name);
    
    public IReadOnlyList<Tag> Tags => _devices.SelectMany(d => d.Tags).ToList();
}

public class Device : Entity
{
    public string Name { get; set; } = string.Empty;

    private readonly List<Tag> _tags = [];
    public IReadOnlyList<Tag> Tags => _tags;

    public void AddTag(Tag tag)
    {
        if (_tags.Any(t => t.Name == tag.Name))
            throw new ArgumentException($"Tag with name {tag.Name} already exists");
        _tags.Add(tag);
    }

    public void RemoveTag(Tag tag)
    {
        if (_tags.All(t => t.Id != tag.Id))
            throw new ArgumentException($"Tag with id {tag.Id} does not exist");
        _tags.Remove(tag);
    }

    private Device() { }

    private Device(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public static Device Create(string name) => new(Guid.NewGuid(), name);
}

public class Tag : TagEntity
{
    public string Name { get; set; } = "标记 1";
    public string Address { get; set; } = "K0001";
    public DataType DataType { get; set; } = DataType.Word;
    public int ScanRate { get; set; } = 100;
    public string Remark { get; set; } = string.Empty;

    public NodeId ToNodeId() => new(Name);

    private Tag() { }

    private Tag(Guid id, string name, string address, DataType dataType, int scanRate, string remark)
        : base(id)
    {
        Name = name;
        Address = address;
        DataType = dataType;
        ScanRate = scanRate;
        Remark = remark;
    }

    /// <summary>
    /// 自定义创建Tag方法
    /// </summary>
    /// <param name="name">通道.设备.组.标签/通道.设备.标签</param>
    /// <param name="address">PLC地址</param>
    /// <param name="dataType">OPC数据类型</param>
    /// <param name="scanRate">扫描频率</param>
    /// <param name="remark">说明</param>
    /// <returns></returns>
    public static Tag Create(
        string name,
        string address,
        DataType dataType,
        int scanRate,
        string remark
    ) => new(Guid.NewGuid(), name, address, dataType, scanRate, remark);
}
