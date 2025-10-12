using OpcUaMonitor.Domain.Shared;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Api.Endpoints;


public class EventLogEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/event-logs")
            .WithTags("EventLog");

        group.MapGet("", GetEventLogs)
            .WithName("GetEventLogs")
            .WithOpenApi();
    }

    private static async Task<IResult> GetEventLogs(
        IUaRepository repository,
        [AsParameters] EventLogFilter filter,
        CancellationToken token)
    {
        var (logs, count) = await repository.GetEventLogsAsync(filter, token);
        return Results.Ok(new { code = 0, msg = "success", data = logs, total = count });
    }
}
