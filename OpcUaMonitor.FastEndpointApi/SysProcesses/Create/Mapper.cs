using OpcUaMonitor.Domain.Sys;

namespace SysProcesses.Create;

internal sealed class Mapper : Mapper<Request, Response, object>
{
    public override Process ToEntity(Request r)
    {
        var process = Process.Create(r.Name, r.Description);

        process.Area = r.Area;

        return process;
    }
}