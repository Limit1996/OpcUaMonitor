using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpcUaMonitor.Domain.Events;
using OpcUaMonitor.Domain.Manager;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Application.Handlers;

public class EventLogCreatedEventHandler : INotificationHandler<EventLogCreatedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventLogCreatedEventHandler> _logger;
    private readonly OpcUaManager _opcUaManager;

    public EventLogCreatedEventHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<EventLogCreatedEventHandler> logger,
        OpcUaManager opcUaManager
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _opcUaManager = opcUaManager;
    }

    public async Task Handle(EventLogCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUaRepository>();

            var eventLog = notification.EventLog;

            if (eventLog is { Event.EventType: EventType.Custom, Event.Name: "CuringStartMonitor" })
            {
                if (eventLog.Value != "True") //非True不记录
                    return;

                var opcUaProvider = _opcUaManager
                    .OpcUaProviders.Single(x => x.Key.Id == eventLog.Event.ChannelId)
                    .Value;

                var fullNodeId = eventLog.Parameters["CurrentNodeId"].ToString();

                var step = await opcUaProvider.ReadAsync<long>(
                    fullNodeId?.Replace("CuringStart", "CuringStep") ?? string.Empty,
                    cancellationToken
                );
                if (step != 1) //非第一步1不记录
                    return;

                var finalValue =
                    $"CuringStart:{eventLog.Value},CuringStep:{step},SourceTimestamp:{eventLog.Parameters["SourceTimestamp"]}";

                var insertLog = EventLog.Create(eventLog.Event, finalValue);
                await repository.AddEventLogAsync(insertLog, cancellationToken);
                return;
            }
            await repository.AddEventLogAsync(notification.EventLog, cancellationToken);
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
