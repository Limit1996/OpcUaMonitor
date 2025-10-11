namespace OpcUaMonitor.Application.Abstractions.Clock;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateOnly Today { get; }
}