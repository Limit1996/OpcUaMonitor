using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Infrastructure;

namespace OpcUaMonitor.FastEndpointApi.Tag.Create;

internal sealed class Endpoint : Endpoint<Request, Response, Mapper>
{
    public OpcDbContext DbContext { get; set; }

    public override void Configure()
    {
        Post("api/opc-tag/create");
        Description(x => x.WithTags("OPC"));
        AllowAnonymous();
        DontCatchExceptions();
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        var opcDevice = await DbContext
            .Set<OpcUaMonitor.Domain.Ua.Device>()
            .Include(d => d.Tags)
            .SingleAsync(d => d.Id == r.OpcDeviceId);

        var tags = Map.ToEntity(r);

        foreach (var tag in tags)
            opcDevice?.AddTag(tag);

        _ = await DbContext.SaveChangesAsync(c);

        await Send.OkAsync(new Response(), c);
    }
}