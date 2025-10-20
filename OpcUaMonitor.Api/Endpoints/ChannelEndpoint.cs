using MediatR;

namespace OpcUaMonitor.Api.Endpoints;

public class ChannelEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/channels").WithTags("Channels");
        group.MapGet("query", GetChannels).WithName("GetChannels").WithOpenApi();
    }
    private static async Task<IResult> GetChannels(
        Guid systemDeviceId,
        IMediator mediator,
        CancellationToken token
    )
    {
        var result = await mediator.Send(
            new Application.Channels.GetChannelsQuery(systemDeviceId),
            token
        );

        return Results.Ok(result.Value);
    }
}