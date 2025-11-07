using App.Core;
using Microsoft.Extensions.DependencyInjection;
using Mix.Core;

namespace Mix.Console.Extensions;

internal static class DbConnectionFactoryExtensions
{
    public static IServiceCollection AddDbConnectionFactory(
        this IServiceCollection services,
        DbType dbType,
        string connectionString)
    {

        services.AddKeyedSingleton<IDbConnectionFactory>(dbType, (sp, _) =>
        {
            return new DbConnectionFactory(connectionString);
        });
        return services;
    }
}
