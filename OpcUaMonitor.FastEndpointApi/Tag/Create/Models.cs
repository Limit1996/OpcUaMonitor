using OpcUaMonitor.Domain.Shared;

namespace Tag.Create;

internal sealed class Request
{
    public Guid OpcDeviceId { get; set; }
    public List<Item> Items { get; set; } = [];

    internal record Item(string Name, string Address, DataType DataType, int ScanRate, string Remark);

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.OpcDeviceId)
                .NotEmpty()
                .WithMessage("设备ID不能为空");
            RuleForEach(r => r.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.Name)
                    .NotEmpty()
                    .WithMessage("标签名称不能为空")
                    .MaximumLength(100)
                    .WithMessage("标签名称长度不能超过100个字符");
                item.RuleFor(i => i.Address)
                    .NotEmpty()
                    .WithMessage("标签地址不能为空")
                    .MaximumLength(200)
                    .WithMessage("标签地址长度不能超过200个字符");
                item.RuleFor(i => i.ScanRate)
                    .GreaterThan(0)
                    .WithMessage("扫描频率必须大于0");
                item.RuleFor(i => i.Remark)
                    .MaximumLength(500)
                    .WithMessage("备注长度不能超过500个字符");
            });
        }
    }
}

internal sealed class Response
{
    public string Message => "v";
}