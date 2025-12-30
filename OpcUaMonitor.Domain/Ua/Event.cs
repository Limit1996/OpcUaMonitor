using OpcUaMonitor.Domain.Abstractions;

namespace OpcUaMonitor.Domain.Ua;

public class Event : Entity
{
    private string _name = string.Empty;

    public string Name => _name;
    public Tag Tag { get; private set; } = null!;
    public Guid TagId { get; private set; }

    public Channel Channel { get; private set; } = null!;
    public Guid ChannelId { get; private set; }

    public bool IsActive { get; set; }

    // 是否已被注册
    // public bool IsRegistered { get; set; }

    public EventType EventType { get; set; }
    public string Remark { get; set; } = string.Empty;

    // EF Core 构造函数
    private Event()
    {
    }

    private Event(Guid id, string name, Guid tagId, string remark, Guid channelId)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be null or empty.");

        _name = name;
        TagId = tagId;
        Remark = remark;
        IsActive = true;
        EventType = EventType.LogToDatabase;
        ChannelId = channelId;
    }

    public static Event Create(string name, Tag tag, string remark, Guid channelId)
    {
        ArgumentNullException.ThrowIfNull(tag);
        return new(Guid.NewGuid(), name, tag.Id, remark, channelId);
    }

    // 改用领域方法
    public EventLog? TryCreateLog(object value)
    {
        return !IsActive
            ? null
            : EventLog.Create(this, value);
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

    public Dictionary<string, object> Parameters { get; set; } = new();
    public object Value { get; private set; }

    private EventLog()
    {
    }

    private EventLog(Guid id, Event @event, object value)
        : base(id)
    {
        Event = @event;
        EventId = @event.Id;
        Value = value;
        Timestamp = DateTime.Now;
    }

    public static EventLog Create(Event @event, object value) => new(Guid.NewGuid(), @event, value);
}

public enum EventType
{
    LogToDatabase,
    LogToFile,
    LogToSms
}