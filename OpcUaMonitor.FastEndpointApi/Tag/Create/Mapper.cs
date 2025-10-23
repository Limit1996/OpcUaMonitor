namespace Tag.Create;

internal sealed class Mapper : Mapper<Request, Response, object>
{
    public List<OpcUaMonitor.Domain.Ua.Tag> ToEntity(Request request)
    {
        var tags = new List<OpcUaMonitor.Domain.Ua.Tag>();
        foreach (var item in request.Items)
        {
            var tag = OpcUaMonitor.Domain.Ua.Tag.Create(
                item.Name,
                item.Address,
                item.DataType,
                item.ScanRate,
                item.Remark
            );
            tags.Add(tag);
        }
        return tags;
    }
}