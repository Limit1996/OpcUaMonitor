using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpcUaMonitor.Domain.Events;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Application.Handlers;

public class EventLogRecordEventHandler : INotificationHandler<EventLogCreatedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventLogRecordEventHandler> _logger;


    public EventLogRecordEventHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<EventLogRecordEventHandler> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(EventLogCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUaRepository>();
            await (notification.EventLog.Event.EventType switch
            {
                EventType.LogToDatabase => repository.AddEventLogAsync(notification.EventLog, cancellationToken),
                EventType.LogToFile or EventType.LogToSms => throw  new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(notification.EventLog.Event.EventType))
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save event log: {EventName}",
                notification.EventLog.EventId
            );
        }
    }
}