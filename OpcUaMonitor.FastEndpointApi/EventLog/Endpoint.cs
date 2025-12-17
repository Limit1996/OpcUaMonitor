using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Infrastructure;

namespace OpcUaMonitor.FastEndpointApi.EventLog;

internal sealed class Endpoint : Endpoint<EventLogRequest, List<EventLogResponse>>
{
    public OpcDbContext DbContext { get; set; }

    public override void Configure()
    {
        Post("api/event-log/list");
        Description(x => x.WithTags("View"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(EventLogRequest req, CancellationToken ct)
    {
        var deviceName = req.DeviceName;
        var tagAddressPattern = $"%{req.TagRemark}%";
        var values = string.Join(",", req.Values ?? []);

        var result = await DbContext
            .Database.SqlQuery<EventLogResponse>(
                $"""
                select t4.Name as DeviceName, t3.Name as TagAddress, t3.Remark as TagRemark, t.Timestamp, t.Value
                from Opc_EventLogs t
                         left join Opc_Events t1 on t.EventId = t1.id
                         left Join Opc_Channels t2 on t2.id = t1.ChannelId
                         left join Opc_Tags t3 on t3.id = t1.TagId
                         left join Opc_Devices t4 on t4.id = t3.DeviceId
                where ({deviceName} is null or t4.Name = {deviceName})
                and ({req.TagRemark} is null or t3.Remark like {tagAddressPattern})
                and ({values} = '' or t.Value in ({values}))
                and t.Timestamp between {req.StartTime} and {req.EndTime}
                order by t.Timestamp desc
                """
            )
            .ToListAsync(ct);

        await Send.OkAsync(result, ct);
    }
}
