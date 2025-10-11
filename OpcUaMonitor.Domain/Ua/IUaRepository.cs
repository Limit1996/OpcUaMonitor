using OpcUaMonitor.Domain.Shared;

namespace OpcUaMonitor.Domain.Ua;

public interface IUaRepository
{
    Task<IEnumerable<Channel>> GetChannelsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<Channel>> GetChannelsAsync(string name, CancellationToken cancellationToken);
    
    Task<IEnumerable<Event>> GetEventsAsync(CancellationToken cancellationToken);
    
    Task<(IEnumerable<EventLog>,int)> GetEventLogsAsync(EventLogFilter filter,CancellationToken cancellationToken);
    
    Task AddEventLogAsync(EventLog log, CancellationToken cancellationToken);
}