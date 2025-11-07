using App.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mix.Console.Extensions;
using Mix.Console.Hosted;
using Mix.Core;
using Mix.Core.Contexts;
using Mix.Core.Services;
using OpcUaMonitor.Domain;
using OpcUaMonitor.Domain.Manager;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddLogging(option =>
{
    option.AddConsole();
    option.AddFile(builder.Configuration.GetSection("Logging"));
});

builder.Services.AddSingleton<OpcUaManager>();

builder.Services.AddSingleton<IServer<PrintContext>, PrintServer>();

builder.Services.AddSingleton<IAppBuilder<PrintContext>, MixPrintBuilder>(sp => new(sp));

builder.Services.AddDbConnectionFactory(DbType.Mes,builder.Configuration.GetConnectionString("MesConnection")!);
builder.Services.AddDbConnectionFactory(DbType.Local,builder.Configuration.GetConnectionString("LocalConnection")!);

builder.Services.AddScoped<IBatchInfoService, BatchInfoService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IOpcUaMonitorDomainFlag).Assembly);
    cfg.LicenseKey = builder.Configuration.GetSection("MediatR:LicenseKey").Value;
});

builder.Services.AddHostedService<OpcUaInitService>();
builder.Services.AddHostedService<PrintInitService>();

using var host = builder.Build();

await host.RunAsync();
