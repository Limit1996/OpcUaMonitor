using App.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mix.Core.Contexts;
using Mix.Core.Services;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaMonitor.Domain.Manager;

namespace Mix.Core;

public class PrintServer : IServer<PrintContext>
{
    private readonly OpcUaManager _opcUaManager;
    private readonly IConfiguration _configuration;
    private readonly IBatchInfoService _batchInfoService;
    private readonly ILogger<PrintServer> _logger;
    public PrintServer(OpcUaManager opcUaManager,
        IConfiguration configuration,
        IBatchInfoService batchInfoService,
        ILogger<PrintServer> logger)
    {
        _opcUaManager = opcUaManager;
        _configuration = configuration;
        _batchInfoService = batchInfoService;
        _logger = logger;
    }

    public ValueTask DisposeAsync()
       => _opcUaManager.DisposeAsync();

    public async Task StartAsync(Func<PrintContext, Task> handler)
    {


        var printChannel = _configuration["Print:Channel"]!;
        var printTag = _configuration["Print:Tag"]!;

        var opcUaProvider = _opcUaManager.OpcUaProviders.FirstOrDefault(x => x.Key.Name == printChannel);

        var session = opcUaProvider.Value.GetSession();

        if (session == null)
            throw new InvalidOperationException("OPC UA 会话未建立！");


        var subscription = new Subscription(session!.DefaultSubscription)
        {
            PublishingInterval = 1000,
            PublishingEnabled = true
        };

        var monitoredItem = new MonitoredItem(subscription.DefaultItem)
        {
            StartNodeId = NodeId.Parse(printTag),
            AttributeId = Attributes.Value,
            SamplingInterval = 1000,
            QueueSize = 0,
            DiscardOldest = true
        };

        monitoredItem.Notification += async (_, eventArgs) =>
        {
            if (eventArgs.NotificationValue is not MonitoredItemNotification notification)
            {
                _logger.LogWarning("收到不支持的通知类型.");
                return;
            }

            //检测数据是否bad
            if (StatusCode.IsBad(notification.Value.StatusCode))
            {
                _logger.LogWarning("标签 {Tag} 数据状态异常: {StatusCode}", monitoredItem.StartNodeId, notification.Value.StatusCode);
                return;
            }
            var value = notification.Value.WrappedValue.Value;
            if (value is bool boolValue && boolValue)
            {
                _logger.LogInformation("检测到打印请求，开始处理打印任务...");
                var weight = await opcUaProvider.Value.ReadAsync<float>("ns=2;s=SplicPlc.Mix.FinalWeight");
                _logger.LogInformation("读取到最终重量: {Weight} KG", weight);
                var batchInfo = await _batchInfoService.GetWaitPrintBatchInfoAsync(
                    new WatiPrintRequest(_configuration["Device:Code"]!));
                var printContext = new PrintContext(batchInfo);
                printContext.Weight = new Entity.Weight(Convert.ToDecimal(weight), "KG");
                await handler(printContext);
            }
            if (value is int intValue && intValue == 1)
            {
                _logger.LogInformation("检测到打印请求，开始处理打印任务...");
                var weight = await opcUaProvider.Value.ReadAsync<float>("ns=2;s=SplicPlc.Mix.FinalWeight");
                _logger.LogInformation("读取到最终重量: {Weight} KG", weight);
                var batchInfo = await _batchInfoService.GetWaitPrintBatchInfoAsync(
                    new WatiPrintRequest(_configuration["Device:Code"]!));
                var printContext = new PrintContext(batchInfo);
                printContext.Weight = new Entity.Weight(Convert.ToDecimal(weight), "KG");
                await handler(printContext);
            }
        };

        var finalWeightMonitoredItem = new MonitoredItem(subscription.DefaultItem)
        {
            StartNodeId = NodeId.Parse("ns=2;s=SplicPlc.Mix.FinalWeight"),
            AttributeId = Attributes.Value,
            SamplingInterval = 1000,
            QueueSize = 0,
            DiscardOldest = true
        };

        finalWeightMonitoredItem.Notification += async (_, eventArgs) =>
        {
            if (eventArgs.NotificationValue is not MonitoredItemNotification notification)
            {
                _logger.LogWarning("收到不支持的通知类型.");
                return;
            }
            //检测数据是否bad
            if (StatusCode.IsBad(notification.Value.StatusCode))
            {
                _logger.LogWarning("标签 {Tag} 数据状态异常: {StatusCode}", monitoredItem.StartNodeId, notification.Value.StatusCode);
                return;
            }
            var value = notification.Value.WrappedValue.Value;
            _logger.LogInformation("实时重量更新: {Weight} KG", value);

            if (value is float floatValue)
            {
                _logger.LogInformation("检测到重量变更打印请求，开始处理打印任务...");
                var batchInfo = await _batchInfoService.GetWaitPrintBatchInfoAsync(
                    new WatiPrintRequest(_configuration["Device:Code"]!));
                var printContext = new PrintContext(batchInfo);
                var decimalValue = Convert.ToDecimal(floatValue);
                printContext.Weight = new Entity.Weight(decimalValue, "KG");
                await handler(printContext);
            }
        };

        subscription.AddItem(monitoredItem);
        //subscription.AddItem(finalWeightMonitoredItem);
        session.AddSubscription(subscription);
        await subscription.CreateAsync();
    }
}
