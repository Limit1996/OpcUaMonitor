namespace OpcUaMonitor.Api.Endpoints;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}