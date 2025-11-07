using App.Core;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mix.Core.Entity;

namespace Mix.Core.Services;

public interface IBatchInfoService
{
    Task<BatchInfo> GetWaitPrintBatchInfoAsync(WatiPrintRequest watiPrintRequest);
}

public class BatchInfoService : IBatchInfoService
{
    private readonly IDbConnectionFactory _mesDbConnectionFactory;
    public BatchInfoService([FromKeyedServices(DbType.Mes)] IDbConnectionFactory mesDbConnectionFactory)
    {
        _mesDbConnectionFactory = mesDbConnectionFactory;
    }
    public async Task<BatchInfo> GetWaitPrintBatchInfoAsync(WatiPrintRequest watiPrintRequest)
    {
        using var connection = _mesDbConnectionFactory.CreateDbConnection();
        const string sql = @"
            SELECT TOP 1
                t.Barcode,
                t.Equip_Code as DeviceCode,
                t1.Shift_Name as ShiftName,
                t2.ShiftName as GroupName,
                t.Mater_code as MaterialCode,
                t.Mater_Name as MaterialName,
                t.Barcode_start as BarcodeStart,
                t.Barcode_end as BarcodeEnd,
                t.Total_weight as StandardWeight
            FROM
                Ppt_ShiftConfig t WITH (NOLOCK)
            LEFT JOIN
                Ppt_Shift t1 WITH (NOLOCK) ON t.Shift_id = t1.ObjId
            LEFT JOIN
                Ppt_Class t2 WITH (NOLOCK) ON t.Shift_class = t2.ObjId
            WHERE
                Barcode_Use = 0
            AND 
                Equip_Code = @DeviceCode
            ORDER BY
                Prod_Time, Barcode_start";
        return await connection.QuerySingleAsync<BatchInfo>(sql, watiPrintRequest);
    }
}

public record WatiPrintRequest(string DeviceCode, int Row = 1);
