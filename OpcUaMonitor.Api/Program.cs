using OpcUaMonitor.Api.Extensions;
using OpcUaMonitor.Api.Hosted;
using OpcUaMonitor.Application;
using OpcUaMonitor.Domain;
using OpcUaMonitor.Domain.Shared;
using OpcUaMonitor.Domain.Ua;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbRepository(builder.Configuration);
builder.Services.AddOpcService();
builder.Services.AddClockService();

builder.Services.AddHostedService<OpcUaHostedService>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IOpcUaMonitorDomainFlag).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(IOpcUaMonitorApplicationFlag).Assembly);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.MapGet("/event-log", async (IUaRepository repository, CancellationToken token) =>
{
    var filter = new EventLogFilter
    {
        PageNumber = 1,
        PageSize = 20
    };

    var (logs,count) = await repository.GetEventLogsAsync(filter, token);
    return Results.Ok(new { code = 0, msg = "success", data = logs, total = count });
});

app.Run();