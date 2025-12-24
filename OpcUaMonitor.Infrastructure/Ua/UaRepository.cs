using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Domain.Shared;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Infrastructure.Ua;

public class UaRepository : Repository<Channel>, IUaRepository
{
    public UaRepository(OpcDbContext dbContext)
        : base(dbContext) { }

    public async Task<IEnumerable<Channel>> GetChannelsAsync(CancellationToken cancellationToken)
    {
        return await DbContext.Set<Channel>().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Channel>> GetChannelsAsync(
        string name,
        CancellationToken cancellationToken
    )
    {
        return await DbContext
            .Set<Channel>()
            .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetEventsAsync(CancellationToken cancellationToken)
    {
        return await DbContext
            .Set<Event>()
            .Where(e => e.IsActive)
            .Include(x => x.Tag)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<EventLog>, int)> GetEventLogsAsync(
        EventLogFilter filter,
        CancellationToken cancellationToken
    )
    {
        var query = DbContext.Set<EventLog>().Where(log => log.Event.EventType == filter.Type);

        if (filter.From.HasValue)
            query = query.Where(log => log.Timestamp >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(log => log.Timestamp <= filter.To.Value);

        if (filter.TagId.HasValue)
            query = query.Where(log => log.Event.TagId == filter.TagId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(log => log.Timestamp)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .Include(log => log.Event)
            .ThenInclude(e => e.Tag)
            .ToListAsync(cancellationToken);


        return (items, totalCount);
    }

    /// <summary>
    /// 待优化：批量插入
    /// </summary>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    public async Task AddEventLogAsync(EventLog log, CancellationToken cancellationToken)
    {
        await DbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"INSERT INTO [Opc_EventLogs] ([Id], [EventId], [Timestamp], [Value], [Parameters])
           VALUES ({log.Id}, {log.EventId}, {log.Timestamp}, {log.Value}, {JsonSerializer.Serialize(log.Parameters)})",
            cancellationToken);
    }

}
