using OpcUaMonitor.Domain.Abstractions;

namespace OpcUaMonitor.Domain.Ua;

public class Event : Entity
{
    private string _name = string.Empty;

    public string Name => _name;
    public Guid TagId { get; private set; }

    // 添加 null-forgiving 或设置默认值
    public Tag Tag { get; private set; } = null!;

    public bool IsActive { get; private set; }
    
    // 是否已被注册
    // public bool IsRegistered { get; set; }
    
    public EventType EventType { get; private set; }
    public string Remark { get; private set; } = string.Empty;

    // EF Core 构造函数
    private Event() { }

    private Event(Guid id, string name, Guid tagId, string remark)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be null or empty.");

        _name = name;
        TagId = tagId;
        Remark = remark;
        IsActive = true;
        EventType = EventType.ValueChanged;
    }

    public static Event Create(string name, Tag tag, string remark)
    {
        ArgumentNullException.ThrowIfNull(tag);
        return new(Guid.NewGuid(), name, tag.Id, remark);
    }

    // 改用领域方法
    public EventLog? TryCreateLog(object value)
    {
        return !IsActive 
            ? null 
            : EventLog.Create(this, value.ToString() ?? string.Empty);
    }

    public void UpdateName(string name)
    {
        if (IsActive)
            throw new InvalidOperationException("Cannot change the name of an active event.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be null or empty.");

        _name = name;
    }
}

public class EventLog : Entity
{
    public Guid EventId { get; private set; }
    public Event Event { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.Now;
    public string Value { get; private set; }

    private EventLog() { }

    private EventLog(Guid id, Event @event, string value)
        : base(id)
    {
        Event = @event;
        EventId = @event.Id;
        Value = value;
        Timestamp = DateTime.Now;
    }

    public static EventLog Create(Event @event, string value) => new(Guid.NewGuid(), @event, value);
}

public enum EventType
{
    ValueChanged,
    Custom,
}
