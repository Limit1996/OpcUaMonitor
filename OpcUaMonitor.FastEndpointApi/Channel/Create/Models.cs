namespace OpcUaMonitor.FastEndpointApi.Channel.Create;

internal sealed class Request
{
    public Guid SysDeviceId { get; set; }
    public List<Item> Items { get; set; } = [];

    internal record Item
    {
        public string OpcUaUrl { get; set; }
        public string Name { get; set; }
    }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.SysDeviceId)
                .NotEmpty()
                .WithMessage("设备ID不能为空");

            RuleForEach(r => r.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.OpcUaUrl)
                    .NotEmpty()
                    .WithMessage("OPC UA地址不能为空")
                    .MaximumLength(200)
                    .WithMessage("OPC UA地址长度不能超过200个字符");
                item.RuleFor(i => i.Name)
                    .NotEmpty()
                    .WithMessage("通道名称不能为空")
                    .MaximumLength(100)
                    .WithMessage("通道名称长度不能超过100个字符");
            });
        }
    }
}

internal sealed class Response
{
    public string Message => "v";
}