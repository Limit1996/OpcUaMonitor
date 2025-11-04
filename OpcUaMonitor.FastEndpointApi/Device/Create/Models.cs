namespace OpcUaMonitor.FastEndpointApi.Device.Create;

internal sealed class Request
{
    public string Name { get; set; } = string.Empty;
    public Guid ChannelId { get; set; }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .WithMessage("设备名称不能为空")
                .MaximumLength(100)
                .WithMessage("设备名称长度不能超过100个字符");
            RuleFor(r => r.ChannelId)
                .NotEmpty()
                .WithMessage("通道ID不能为空");
        }
    }
}

internal sealed class Response
{
    public string Message => "v";
}