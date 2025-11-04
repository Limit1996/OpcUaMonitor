using OpcUaMonitor.Domain.Shared;

namespace OpcUaMonitor.FastEndpointApi.Tree;

internal sealed class TreeRequest
{
    public Flag Flag { get; set; }

    public Area Area { get; set; }

    public Guid? Id { get; set; } = Guid.Empty;

    internal sealed class Validator : Validator<TreeRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Flag)
                .IsInEnum()
                .WithMessage("Flag值无效");

            RuleFor(r => r.Area)
                .IsInEnum()
                .WithMessage("Area值无效");

            
        }
    }
}

internal sealed class TreeResponse
{
    public Guid Id { get; set; }
    public string Label { get; set; }
    public bool Leaf { get; set; }
}

enum Flag
{
    Process,
    SysDevice,
    OpcChannel,
    OpcDevice,
    Tag
}