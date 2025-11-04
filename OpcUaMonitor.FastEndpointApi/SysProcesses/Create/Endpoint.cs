using OpcUaMonitor.Domain.Sys;
using OpcUaMonitor.Infrastructure;

namespace OpcUaMonitor.FastEndpointApi.SysProcesses.Create;

internal sealed class Endpoint : Endpoint<Request, Response, Mapper>
{
    public OpcDbContext dbContext { get; set; }

    public override void Configure()
    {
        Post("/api/sys-process/create");
        Description(x => x.WithTags("System"));
        AllowAnonymous();
        DontCatchExceptions();
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        dbContext.Add<Process>(Map.ToEntity(r));

        _ = await dbContext.SaveChangesAsync(c);

        await Send.OkAsync(new Response(), c);
    }
}