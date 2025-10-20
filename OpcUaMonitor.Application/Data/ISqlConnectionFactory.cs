using System.Data;

namespace OpcUaMonitor.Application.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}