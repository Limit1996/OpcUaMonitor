using Dapper;
using MediatR;
using OpcUaMonitor.Application.Data;
using OpcUaMonitor.Domain.Abstractions;

namespace OpcUaMonitor.Application.Channels;

public class GetChannelsHandler : IRequestHandler<GetChannelsQuery, Result<List<ChannelResponse>>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GetChannelsHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<List<ChannelResponse>>> Handle(
        GetChannelsQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _connectionFactory.CreateConnection();
        var query =
                @"-- noinspection SqlNoDataSourceInspection
                SELECT Id,
                       Name,
                       Url As OpcUrl
                FROM Opc_Channels
                where SysDeviceId = @SysDeviceId";

        var channels = (
            await connection.QueryAsync<ChannelResponse>(query, new { SysDeviceId = request.SystemDeviceId })
        ).ToList();

        if (channels.Count == 0)
        {
            return Result.Failure<List<ChannelResponse>>(
                new Error(
                    "Channel.NotFound",
                    "该设备未配置任何OPC通道信息。"
                )
            );
        }

        return channels;
    }
}
