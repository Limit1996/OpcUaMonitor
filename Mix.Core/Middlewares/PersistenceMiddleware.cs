using App.Core;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mix.Core.Contexts;

namespace Mix.Core.Middlewares;

public class PersistenceMiddleware
{
    private readonly Func<PrintContext, Task> _next;
    private readonly ILogger<PersistenceMiddleware> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public PersistenceMiddleware(
        Func<PrintContext, Task> next,
        ILogger<PersistenceMiddleware> logger,
        [FromKeyedServices(DbType.Local)] IDbConnectionFactory dbConnectionFactory
    )
    {
        _next = next;
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InvokeAsync(PrintContext context)
    {
        _logger.LogInformation("PersistenceMiddleware: 正在持久化 PrintContext...");

        using var connection = _dbConnectionFactory.CreateDbConnection();

        const string sql =
            @"
            INSERT INTO T_BatchInfo (Barcode, DeviceCode, ShiftName, GroupName, MaterialCode, MaterialName, BarcodeStart,
                         BarcodeEnd, StandardWeight, FinalWeight, ContainerNo)
            VALUES (@Barcode, @DeviceCode, @ShiftName, @GroupName, @MaterialCode, @MaterialName, @BarcodeStart,
                         @BarcodeEnd, @StandardWeight, @FinalWeight, @ContainerNo)";

        await connection.ExecuteAsync(
            sql,
            new
            {
                context.BatchInfo.Barcode,
                context.BatchInfo.DeviceCode,
                context.BatchInfo.ShiftName,
                context.BatchInfo.GroupName,
                context.BatchInfo.MaterialCode,
                context.BatchInfo.MaterialName,
                context.BatchInfo.BarcodeStart,
                context.BatchInfo.BarcodeEnd,
                context.BatchInfo.StandardWeight,
                FinalWeight = context.Weight.Value,
                ContainerNo = context.Container!.Value,
            }
        );

        await _next(context);
    }
}
