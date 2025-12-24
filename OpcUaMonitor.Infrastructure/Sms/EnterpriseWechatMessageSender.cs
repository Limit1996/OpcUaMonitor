using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpcUaMonitor.Application.Abstractions.Sms;

namespace OpcUaMonitor.Infrastructure.Sms;

public class EnterpriseWechatMessageSender : IMessageSender
{
    private readonly ILogger<EnterpriseWechatMessageSender> _logger;

    private readonly IConfiguration _configuration;

    private readonly HttpClient _httpClient;

    public EnterpriseWechatMessageSender(
        IConfiguration configuration,
        ILogger<EnterpriseWechatMessageSender> logger,
        HttpClient httpClient
    )
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public Task SendAsync(string subject, string body)
    {
        var sendUsers = _configuration.GetSection("Sms:EnterpriseWechat:UserIds").Value;

        bool isNullOrInvalid = string.IsNullOrWhiteSpace(sendUsers) || !sendUsers.Contains('|');

        if (isNullOrInvalid)
        {
            throw new InvalidOperationException("EnterpriseWechat UserIds 配置错误");
        }
        
        var content = string.IsNullOrWhiteSpace(subject) ? body : $"{subject}\n{body}";
        
        return SendHttpRequestAsync(sendUsers!, content);
    }
    
    private Task SendHttpRequestAsync(string users, string content, int waitSeconds = 0)
    {
        var message = new
        {
            Users = users,
            Content = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] : {content}",
            WaitSeconds = waitSeconds
        };
        return _httpClient.PostAsJsonAsync("api/wechat/send", message);
    }
}
