using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Application.Abstractions.Clock;
using OpcUaMonitor.Domain.Manager;
using OpcUaMonitor.Domain.Ua;
using OpcUaMonitor.Infrastructure;
using OpcUaMonitor.Infrastructure.Clock;
using OpcUaMonitor.Infrastructure.Ua;

namespace OpcUaMonitor.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDbRepository(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<OpcDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.LogTo(Console.WriteLine);
        });

        services.AddScoped<IUaRepository, UaRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddOpcService(this IServiceCollection services)
    {
        services.AddSingleton<IOpcUaProvider, OpcUaProvider>();
        services.AddSingleton<OpcUaManager>();
        
        return services;
    }
    
    public static IServiceCollection AddClockService(this IServiceCollection services)
    {
        services.AddTransient<IDateTimeProvider, NowDateTimeProvider>();
        return services;
    }
}