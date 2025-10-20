namespace OpcUaMonitor.Domain.Abstractions;

public record Error(string Code, string Name)
{
    public static readonly Error None = new Error(string.Empty, string.Empty);

    public static readonly Error NullValue = new Error("Error.NullValue", "Null value was provided");
}