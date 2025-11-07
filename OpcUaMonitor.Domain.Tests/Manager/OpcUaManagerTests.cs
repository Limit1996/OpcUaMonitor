using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using OpcUaMonitor.Domain.Events;
using OpcUaMonitor.Domain.Manager;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Tests.Manager;

public class OpcUaManagerTests
{
    private OpcUaManager _opcUaManager;
    private ConnectionLostHandler _connectionLostHandler;

    private IMediator _mediator;
    private ILogger<OpcUaManager> _logger;
    private ILoggerFactory _loggerFactory;
    private Channel _channel = Channel.Create("opc.tcp://localhost:49320", "TestChannel");


    [TearDown]
    public async Task TearDown()
    {
        if (_opcUaManager != null)
        {
            await _opcUaManager.DisposeAsync();
            _opcUaManager = null;
        }
    }

    [SetUp]
    public void Setup()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<OpcUaManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        _mediator = mediatorMock.Object;
        _logger = loggerMock.Object;
        _loggerFactory = loggerFactoryMock.Object;

        _opcUaManager = new OpcUaManager(_mediator, _logger, _loggerFactory);

        // 使用手写的测试替身，避免 Moq/Castle 代理报错
        _opcUaManager.OpcUaProviders.Add(_channel, new OpcUaProvider(_mediator, new Mock<ILogger<OpcUaProvider>>().Object));
        
        _connectionLostHandler = new ConnectionLostHandler(
            _mediator,
            new Mock<ILogger<ConnectionLostHandler>>().Object,
            _loggerFactory,
            _opcUaManager
        );
    }

    [Test]
    public async Task OpcUaManager_Constructor_ShouldInitializeProperties()
    {
        var notification = new ConnectionLostEvent(_channel);
        await _connectionLostHandler.Handle(notification, CancellationToken.None);
        
        var result = _opcUaManager.OpcUaProviders.ContainsKey(_channel);
        Assert.That(result, Is.True);
        
        var provider = _opcUaManager.OpcUaProviders[_channel];
        Assert.That(provider, Is.Not.Null);
        Assert.That(provider.IsConnected, Is.True);
        
        var readValue = await provider.ReadAsync<ushort>("ns=2;s=通道 1.设备 1.标记 2", CancellationToken.None);
        
        Assert.That(readValue, Is.EqualTo(9));

        Assert.That(_opcUaManager.OpcUaProviders.Count, Is.EqualTo(1));

        await Task.Delay(TimeSpan.FromHours(1));
    }
}