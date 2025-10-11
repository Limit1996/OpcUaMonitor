using OpcUaMonitor.Domain.Shared;

namespace OpcUaMonitor.Domain.Abstractions;

public abstract class TagEntity : Entity
{
    protected TagEntity(Guid id) : base(id)
    {
    }
    protected TagEntity()
    {
    }
    public Value Value { get; set; } = new();
}