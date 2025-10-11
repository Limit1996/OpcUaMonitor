using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpcUaMonitor.Domain.Events;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Application.Handlers;

public class EventLogCreatedEventHandler : INotificationHandler<EventLogCreatedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventLogCreatedEventHandler> _logger;

    public EventLogCreatedEventHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<EventLogCreatedEventHandler> logger)
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
            await repository.AddEventLogAsync(notification.EventLog, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save event log: {EventName}", notification.EventLog.EventId);
        }
    }
}