namespace OpcUaMonitor.FastEndpointApi.Device.Create;

internal sealed class Mapper : Mapper<Request, Response, object>
{
    public override OpcUaMonitor.Domain.Ua.Device ToEntity(Request r)
    {
        var device = OpcUaMonitor.Domain.Ua.Device.Create(
            r.Name
        );
        return device;
    }
}