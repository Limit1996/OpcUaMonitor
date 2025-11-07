using System.Data;

namespace App.Core;

public interface IDbConnectionFactory
{
    IDbConnection CreateDbConnection();
}
