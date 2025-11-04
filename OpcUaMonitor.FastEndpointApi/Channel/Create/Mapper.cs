namespace OpcUaMonitor.FastEndpointApi.Channel.Create;

internal sealed class Mapper : Mapper<Request, Response, object>
{
    public override List<OpcUaMonitor.Domain.Ua.Channel> ToEntity(Request r)
    {
        var channels = new List<OpcUaMonitor.Domain.Ua.Channel>();
        foreach (var item in r.Items)
        {
            var channel = OpcUaMonitor.Domain.Ua.Channel.Create(
                item.OpcUaUrl,
                item.Name
            );
            channels.Add(channel);
        }
        return channels;
    }
}