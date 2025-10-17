using System.Diagnostics.CodeAnalysis;
using OpcUaMonitor.Domain.Ua;


namespace OpcUaMonitor.Domain.Shared;

public class EventLogFilter
{
    private int _pageNumber = 1;
    private int _pageSize = 20;

    public EventType Type { get; set; } = EventType.ValueChanged;

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? TagId { get; set; }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 20,
            > 100 => 100, // 限制最大值
            _ => value
        };
    }

    // 验证方法
    public void Validate()
    {
        if (From.HasValue && To.HasValue && To.Value < From.Value)
        {
            throw new ArgumentException("End time cannot be earlier than start time.");
        }
    }

    // 辅助属性
    public int Skip => (PageNumber - 1) * PageSize;
}

public record ProcessFilter(string Name, Area Area);

public record SysDeviceFilter(
    string Name,
    [StringSyntax(StringSyntaxAttribute.Uri)]
    string IpAddress);