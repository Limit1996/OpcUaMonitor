using OpcUaMonitor.Api.Exceptions;
using OpcUaMonitor.Api.Extensions;
using OpcUaMonitor.Api.Hubs;
using OpcUaMonitor.Domain.Shared;
using OpcUaMonitor.Domain.Ua;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbRepository(builder.Configuration);
builder.Services.AddOpcService();
builder.Services.AddClockService();
builder.Services.AddHostedService();
builder.Services.AddMediatorService(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();

builder.Services.AddSignalR();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.MapEndpoints();
// app.MapHub<OpcUaHub>("/opc-ua-hub");

app.Run();