using Microsoft.EntityFrameworkCore;
using OpcUaMonitor.Domain.Abstractions;

namespace OpcUaMonitor.Infrastructure.Ua;

public abstract class Repository<T>
    where T : Entity
{
    protected readonly OpcDbContext DbContext;

    protected Repository(OpcDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<T>()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public virtual void Add(T entity)
    {
        DbContext.Add(entity);
    }
}