using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Api.Endpoints;
using OpcUaMonitor.Api.Hosted;
using OpcUaMonitor.Application;
using OpcUaMonitor.Application.Abstractions.Clock;
using OpcUaMonitor.Application.Abstractions.Sms;
using OpcUaMonitor.Application.Data;
using OpcUaMonitor.Domain;
using OpcUaMonitor.Domain.Manager;
using OpcUaMonitor.Domain.Ua;
using OpcUaMonitor.Infrastructure;
using OpcUaMonitor.Infrastructure.Clock;
using OpcUaMonitor.Infrastructure.Data;
using OpcUaMonitor.Infrastructure.Sms;
using OpcUaMonitor.Infrastructure.Ua;

namespace OpcUaMonitor.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDbRepository(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<OpcDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            // options.LogTo(Console.WriteLine);
        });

        services.AddScoped<IUaRepository, UaRepository>();

        services.AddSingleton<ISqlConnectionFactory>(_ =>
        {
            var connString = configuration.GetConnectionString("DefaultConnection");
            return connString == null
                ? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found."
                )
                : new SqlConnectionFactory(connString);
        });

        return services;
    }

    public static IServiceCollection AddOpcService(this IServiceCollection services)
    {
        services.AddScoped<IOpcUaProvider, OpcUaProvider>();
        services.AddSingleton<OpcUaManager>();

        return services;
    }

    public static IServiceCollection AddClockService(this IServiceCollection services)
    {
        services.AddTransient<IDateTimeProvider, NowDateTimeProvider>();
        return services;
    }

    public static IServiceCollection AddMessageSender(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient<IMessageSender, EnterpriseWechatMessageSender>(client =>
        {
            if (string.IsNullOrEmpty(configuration["Sms:EnterpriseWechat:HttpUrl"]))
                throw new InvalidOperationException("Enterprise Wechat HttpUrl configuration is missing.");

            client.BaseAddress = new Uri(configuration["Sms:EnterpriseWechat:HttpUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        return services;
    }

    public static IServiceCollection AddMediatorService(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IOpcUaMonitorDomainFlag).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(IOpcUaMonitorApplicationFlag).Assembly);
            cfg.LicenseKey = configuration.GetSection("MediatR:LicenseKey").Value;
        });
        return services;
    }

    public static IServiceCollection AddHostedService(this IServiceCollection services)
    {
        services.AddHostedService<OpcUaHostedService>();
        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointType = typeof(IEndpoint);

        var assembly = typeof(Program).Assembly;

        var endpointTypes = assembly
            .GetExportedTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(endpointType)
            );

        foreach (var type in endpointTypes)
        {
            var method = type.GetMethod(
                nameof(IEndpoint.MapEndpoint),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            );

            method?.Invoke(null, [app]);
        }

        return app;
    }
}