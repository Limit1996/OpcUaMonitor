namespace OpcUaMonitor.FastEndpointApi.EventLog;

internal sealed class EventLogRequest
{
    public string? DeviceName { get; set; }

    public string? TagRemark { get; set; }

    public string[]? Values { get; set; }

    public DateTime? StartTime { get; set; } = DateTime.Now.AddDays(-1);
    
    public DateTime? EndTime { get; set; } = DateTime.Now;

    internal sealed class Validator : Validator<EventLogRequest>
    {
        public Validator()
        {
            RuleFor(r => r.DeviceName)
                .MaximumLength(200)
                .WithMessage("DeviceName长度不能超过200个字符");

            RuleFor(r => r.TagRemark)
                .MaximumLength(200)
                .WithMessage("TagRemark长度不能超过200个字符");
        }
    }
}

internal sealed class EventLogResponse
{
    public string? DeviceName { get; set; }
    public string? TagAddress { get; set; }
    public string? TagRemark { get; set; }
    public string? Value { get; set; }
    public DateTime Timestamp { get; set; }
}