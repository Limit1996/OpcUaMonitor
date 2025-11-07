using App.Core;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Mix.Core;

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateDbConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}

//public static class DbConnectionFactoryExtensions
//{
//    public static IServiceCollection AddDbConnectionFactory(
//        this IServiceCollection services,
//        DbType dbType,
//        string connectionString)
//    {

//        services.AddKeyedSingleton<IDbConnectionFactory>(dbType, (sp, _) =>
//        {
//            var config = sp.GetRequiredService<IConfiguration>();
//            return new DbConnectionFactory(config);
//        });
//        return services;
//    }
//}

public enum DbType
{
    Local,
    Mes
}