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
        CancellationToken token
    )
    {
        await provider.ConnectAsync(_ => { }, Channel.Create(url, "temp"), token);

        if (tagNames.Length == 1)
        {
            var result = await provider.ReadAsync<object>(tagNames.FirstOrDefault()!, token);
            return Results.Ok(result);
        }

        var multipleResult = await provider.ReadMultipleAsync(tagNames, token);
        return Results.Ok(multipleResult);
    }
}