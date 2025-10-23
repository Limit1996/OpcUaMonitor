using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Infrastructure;

namespace Device.Create;

internal sealed class Endpoint : Endpoint<Request, Response, Mapper>
{
    public OpcDbContext DbContext { get; set; } = null!;

    public override void Configure()
    {
        Post("api/opc-device/create");
        Description(x => x.WithTags("OPC"));
        AllowAnonymous();
        DontCatchExceptions();
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        var channel = await DbContext
            .Set<OpcUaMonitor.Domain.Ua.Channel>()
            .Include(c => c.Devices)
            .SingleAsync(c => c.Id == r.ChannelId, c);

        var device = Map.ToEntity(r);

        channel.AddDevice(device);

        _ = await DbContext.SaveChangesAsync(c);

        await Send.OkAsync(new Response());
    }
}