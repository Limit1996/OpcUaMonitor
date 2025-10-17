using OpcUaMonitor.Domain.Shared;

namespace OpcUaMonitor.Domain.Sys;

public interface ISysProcessRepository
{
    Task<bool> UpdateAsync(Process[] process, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid[] ids, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Process>> ListAsync(ProcessFilter filter,CancellationToken cancellationToken = default);
}

public interface ISysDeviceRepository
{
    Task<bool> UpdateAsync(Device[] devices, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid[] ids, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Device>> ListAsync(SysDeviceFilter filter,CancellationToken cancellationToken = default);
}