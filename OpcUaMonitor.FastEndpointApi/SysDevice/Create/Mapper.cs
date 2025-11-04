namespace OpcUaMonitor.FastEndpointApi.SysDevice.Create;

internal sealed class Mapper : Mapper<Request, Response, object>
{
    public override OpcUaMonitor.Domain.Sys.Device ToEntity(Request r)
    {
        var device = OpcUaMonitor.Domain.Sys.Device.Create(
            r.Code,
            r.Name,
            r.Manufacturer,
            r.IpAddress,
            r.Specification
        );

        device.ProcessId = r.ProcessId;

        return device;
    }
}