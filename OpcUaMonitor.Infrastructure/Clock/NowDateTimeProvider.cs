using OpcUaMonitor.Application.Abstractions.Clock;

namespace OpcUaMonitor.Infrastructure.Clock;

public class NowDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}