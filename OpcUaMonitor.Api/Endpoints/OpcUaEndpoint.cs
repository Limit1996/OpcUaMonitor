using OpcUaMonitor.Domain.Manager;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Api.Endpoints;

public class OpcUaEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/opcua").WithTags("OPC UA");

        group.MapGet("/read", Read).WithName("ReadAsync").WithOpenApi();
    }

    private static async Task<IResult> Read(
        string url,
        string[] tagNames,
        IOpcUaProvider provider,
        OpcUaManager manager,
        CancellationToken token
    )
    {
        var channel = Channel.Create(url, "temp");

        var queryChannel = manager.OpcUaProviders.Keys.FirstOrDefault(c => c.Url == channel.Url);

        if (queryChannel != null)
        {
            var scopedProvider = manager.OpcUaProviders[queryChannel];
            if (tagNames.Length == 1)
            {
                var scopedResult = await scopedProvider.ReadAsync<object>(
                    tagNames.FirstOrDefault()!,
                    token
                );
                return Results.Ok(scopedResult);
            }

            var scopedMultipleResult = await scopedProvider.ReadMultipleAsync(tagNames, token);
            return Results.Ok(scopedMultipleResult);
        }

        await provider.ConnectAsync(_ => { }, channel, token);

        if (tagNames.Length == 1)
        {
            var result = await provider.ReadAsync<object>(tagNames.FirstOrDefault()!, token);
            return Results.Ok(result);
        }

        var multipleResult = await provider.ReadMultipleAsync(tagNames, token);
        return Results.Ok(multipleResult);
    }
}
