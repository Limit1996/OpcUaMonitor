using OpcUaMonitor.Infrastructure;

namespace OpcUaMonitor.FastEndpointApi.SysDevice.Create;

internal sealed class Endpoint : Endpoint<Request, Response, Mapper>
{
    public OpcDbContext DbContext { get; set; }

    public override void Configure()
    {
        Post("/api/sys-device/create");
        Description(x => x.WithTags("System"));
        AllowAnonymous();
        DontCatchExceptions();
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        var device = Map.ToEntity(r);
        DbContext.Add<OpcUaMonitor.Domain.Sys.Device>(device);

        _ = await DbContext.SaveChangesAsync(c);

        await Send.OkAsync(new Response(), c);
    }
}