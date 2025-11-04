using OpcUaMonitor.Domain.Shared;

namespace OpcUaMonitor.FastEndpointApi.SysProcesses.Query;

internal sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/api/sys-areas");
        Description(x => x.WithTags("System"));
        AllowAnonymous();
        DontCatchExceptions();
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var res = Enum.GetValues(typeof(Area))
            .Cast<Area>()
            .Select(t => new Response.Item((int)t, t.ToString().ToUpper()))
            .ToList();

        await Send.OkAsync(new Response() { Areas = res }, c);
    }
}