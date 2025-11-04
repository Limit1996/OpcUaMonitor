using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Infrastructure;

namespace OpcUaMonitor.FastEndpointApi.Channel.Create;

internal sealed class Endpoint : Endpoint<Request, Response, Mapper>
{
    public OpcDbContext DbContext { get; set; }

    public override void Configure()
    {
        Post("api/opc-channel/create");
        Description(x => x.WithTags("OPC"));
        AllowAnonymous();
        DontCatchExceptions();
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        var device = await DbContext
            .Set<OpcUaMonitor.Domain.Sys.Device>()
            .Include(d => d.Channels)
            .SingleAsync(d => d.Id == r.SysDeviceId, c);
        var channels = Map.ToEntity(r);
        foreach (var channel in channels)
            device?.AddChannel(channel);
        _ = await DbContext.SaveChangesAsync(c);
        await Send.OkAsync(new Response(), c);
    }
}