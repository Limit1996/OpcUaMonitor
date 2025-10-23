using OpcUaMonitor.Domain.Shared;

namespace SysProcesses.Create;

internal sealed class Request
{
    public string Name { get; set; }

    public Area Area { get; set; }

    public string Description { get; set; }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("工序名称不能为空");
            RuleFor(x => x.Area).IsInEnum().WithMessage("工序区域需枚举");
        }
    }
}

internal sealed class Response
{
    public string Message => "v";
}