namespace OpcUaMonitor.Application.Abstractions.Sms;

public interface IMessageSender
{
    Task SendAsync(string subject, string body);
}