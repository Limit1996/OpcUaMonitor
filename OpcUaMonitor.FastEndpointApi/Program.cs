global using FastEndpoints;
global using FluentValidation;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();
builder.Services.AddFastEndpoints().SwaggerDocument();

builder.Services.AddDbContext<OpcUaMonitor.Infrastructure.OpcDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();
app.UseDefaultExceptionHandler().UseFastEndpoints().UseSwaggerGen();
app.Run();