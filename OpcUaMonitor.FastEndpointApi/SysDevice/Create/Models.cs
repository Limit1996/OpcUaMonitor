namespace SysDevice.Create;

internal sealed class Request
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Manufacturer { get; set; }

    public string IpAddress { get; set; }

    public string Specification { get; set; }

    public Guid ProcessId { get; set; }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("设备编码不能为空")
                .MaximumLength(50)
                .WithMessage("设备编码长度不能超过50个字符");
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("设备名称不能为空")
                .MaximumLength(100)
                .WithMessage("设备名称长度不能超过100个字符");
            RuleFor(x => x.Manufacturer)
                .NotEmpty()
                .WithMessage("制造商不能为空")
                .MaximumLength(100)
                .WithMessage("制造商长度不能超过100个字符");
            RuleFor(x => x.IpAddress)
                .NotEmpty()
                .WithMessage("IP地址不能为空")
                .MaximumLength(45)
                .WithMessage("IP地址长度不能超过45个字符"); // IPv6 max length
            RuleFor(x => x.Specification)
                .MaximumLength(500)
                .WithMessage("规格说明长度不能超过500个字符");
        }
    }
}

internal sealed class Response
{
    public string Message => "v";
}