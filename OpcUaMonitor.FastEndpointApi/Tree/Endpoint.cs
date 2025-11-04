using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Domain.Sys;
using OpcUaMonitor.Infrastructure;

namespace OpcUaMonitor.FastEndpointApi.Tree;

internal sealed class Endpoint: Endpoint<TreeRequest, List<TreeResponse>>
{
    public OpcDbContext DbContext { get; set; }
    public override void Configure()
    {
        Post("api/tree/list");
        Description(x => x.WithTags("View"));
        AllowAnonymous();
        DontCatchExceptions();
    }
    
    public override async Task HandleAsync(TreeRequest r, CancellationToken c)
    {
        List<TreeResponse> result = [];

        switch (r.Flag)
        {
            case Flag.Process:
                result = await DbContext
                    .Set<Process>()
                    .Where(p=>p.Area == r.Area)
                    .Select(p => new TreeResponse
                    {
                        Id = p.Id,
                        Label = p.Name,
                    })
                    .AsNoTracking()
                    .ToListAsync(c);
                break;
            case Flag.SysDevice:
                result = await DbContext
                    .Set<Domain.Sys.Device>()
                    .Where(d => d.ProcessId == r.Id)
                    .Select(d => new TreeResponse
                    {
                        Id = d.Id,
                        Label = d.Name,
                    })
                    .AsNoTracking()
                    .ToListAsync(c);
                break;
            case Flag.OpcChannel:
                result = await DbContext
                    .Set<Domain.Ua.Channel>()
                    .FromSqlInterpolated($"SELECT * FROM Opc_Channels WHERE SysDeviceId = {r.Id}")
                    .Select(ch => new TreeResponse
                    {
                        Id = ch.Id,
                        Label = ch.Name,
                    })
                    .AsNoTracking()
                    .ToListAsync(c);
                break;
            case Flag.OpcDevice:
                result = await DbContext
                    .Set<Domain.Ua.Device>()
                    .FromSqlInterpolated($"SELECT * FROM Opc_Devices WHERE ChannelId = {r.Id}")
                    .Select(d => new TreeResponse
                    {
                        Id = d.Id,
                        Label = d.Name,
                        Leaf = true,
                    })
                    .AsNoTracking()
                    .ToListAsync(c);
                break;
            // case Flag.Tag:
            //     result = await DbContext
            //         .Set<Domain.Ua.Tag>()
            //         .FromSqlInterpolated($"SELECT * FROM Opc_Tags WHERE DeviceId = {r.Id}")
            //         .Select(t => new TreeResponse
            //         {
            //             Id = t.Id,
            //             Label = t.Name,
            //             Leaf = true,
            //         })
            //         .AsNoTracking()
            //         .ToListAsync(c);
            //     break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        await Send.OkAsync(result, c);
    }
}