namespace OpcUaMonitor.FastEndpointApi.SysProcesses.Query;

internal sealed class Response
{
    public List<Item> Areas { get; set; }

    internal record Item(int Code, string Name);
}