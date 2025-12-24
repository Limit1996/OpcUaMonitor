using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OpcUaMonitor.Infrastructure.Sms;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpcUaMonitor.Infrastructure.Tests.Sms;

[TestFixture]
[TestOf(typeof(EnterpriseWechatMessageSender))]
public class EnterpriseWechatMessageSenderTest
{
    private Mock<IConfiguration> _configurationMock;
    private Mock<IConfigurationSection> _configSectionMock;
    private Mock<ILogger<EnterpriseWechatMessageSender>> _loggerMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private EnterpriseWechatMessageSender _sender;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configSectionMock = new Mock<IConfigurationSection>();
        _loggerMock = new Mock<ILogger<EnterpriseWechatMessageSender>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        // _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        // {
        //     BaseAddress = new Uri("http://10.10.3.163:2030")
        // };
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://10.10.3.163:2030")
        };

        _sender = new EnterpriseWechatMessageSender(
            _configurationMock.Object,
            _loggerMock.Object,
            _httpClient
        );
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [Test]
    public async Task SendAsync_WithValidUserIds_ShouldSendHttpRequest()
    {
        // Arrange
        _configSectionMock.Setup(x => x.Value).Returns("1032244|1011871");
        _configurationMock
            .Setup(x => x.GetSection("Sms:EnterpriseWechat:UserIds"))
            .Returns(_configSectionMock.Object);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _sender.SendAsync("测试主题", "测试内容");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("api/wechat/send")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Test]
    public void SendAsync_WithNullUserIds_ShouldThrowException()
    {
        // Arrange
        _configSectionMock.Setup(x => x.Value).Returns((string)null!);
        _configurationMock
            .Setup(x => x.GetSection("Sms:EnterpriseWechat:UserIds"))
            .Returns(_configSectionMock.Object);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _sender.SendAsync("测试主题", "测试内容")
        );
    }

    [Test]
    public void SendAsync_WithInvalidUserIds_ShouldThrowException()
    {
        // Arrange
        _configSectionMock.Setup(x => x.Value).Returns("userWithoutPipe");
        _configurationMock
            .Setup(x => x.GetSection("Sms:EnterpriseWechat:UserIds"))
            .Returns(_configSectionMock.Object);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _sender.SendAsync("测试主题", "测试内容")
        );
    }

    [Test]
    public async Task SendAsync_WithEmptySubject_ShouldSendBodyOnly()
    {
        // Arrange
        _configSectionMock.Setup(x => x.Value).Returns("user1|user2");
        _configurationMock
            .Setup(x => x.GetSection("Sms:EnterpriseWechat:UserIds"))
            .Returns(_configSectionMock.Object);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _sender.SendAsync("", "仅有内容");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
