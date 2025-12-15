using MediatR;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaMonitor.Domain.Events;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Manager;

public class OpcUaProvider : IOpcUaProvider
{
    private Session? _session;
    private Channel? _channel;
    private Subscription? _subscription;
    private readonly IMediator _mediator;
    private readonly ILogger<OpcUaProvider> _logger;

    /// <summary>
    /// 这里最好不要使用public，因为Session的创建是异步的，使用工厂模式会更好
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public OpcUaProvider(IMediator mediator, ILogger<OpcUaProvider> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public bool IsConnected => _session is { Connected: true };

    public Session? GetSession() => _session;

    /// <summary>
    /// OPC UA 连接
    /// </summary>
    /// <param name="config"></param>
    /// <param name="channel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ConnectAsync(
        Action<ApplicationConfiguration> config,
        Channel channel,
        CancellationToken cancellationToken = default
    )
    {
        if (IsConnected)
            return true;

        var internalConfig = new ApplicationConfiguration
        {
            ApplicationName = "OpcUaMonitor",
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier(),
                AutoAcceptUntrustedCertificates = true,
            },
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
        };

        config(internalConfig);

        var endpointDescription = await CoreClientUtils.SelectEndpointAsync(
            internalConfig,
            channel.Url,
            false,
            cancellationToken
        );
        var endpointConfiguration = EndpointConfiguration.Create(internalConfig);
        var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

        _session = await Session.CreateAsync(
            internalConfig,
            null,
            endpoint,
            false,
            false,
            internalConfig.ApplicationName,
            (uint)internalConfig.ClientConfiguration.DefaultSessionTimeout,
            new UserIdentity(new AnonymousIdentityToken()),
            null,
            cancellationToken
        );

        var isConnected = _session?.Connected == true;
        if (!isConnected)
            return isConnected;
        _channel = channel;
        _session!.KeepAlive += Session_KeepAlive;
        _session.KeepAliveInterval = 10000; // 10秒心跳
        return isConnected;
    }

    private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        if (e.CurrentState == ServerState.Running)
            return;
        _logger.LogWarning("OPC UA 连接已断开，服务器状态: {Status}", e.CurrentState);
        Task.Run(async () =>
            {
                _logger.LogDebug("正在处理断开连接事件...");
                await DisconnectAsync();
                await _mediator.Publish(new ConnectionLostEvent(_channel!));
                _logger.LogDebug("断开连接事件处理完成.");
            })
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// OPC UA 断开连接
    /// </summary>
    public async ValueTask DisconnectAsync()
    {
        if (_session != null)
        {
            _session.KeepAlive -= Session_KeepAlive;
        }

        if (_session is { Connected: true })
        {
            if (_subscription != null)
            {
                await _session.RemoveSubscriptionAsync(_subscription);
                await _subscription.DeleteAsync(true);
                _subscription = null;
            }

            if (_session.Subscriptions != null)
            {
                foreach (var sub in _session.Subscriptions)
                {
                    await _session.RemoveSubscriptionAsync(sub);
                    await sub.DeleteAsync(true);
                }
            }

            await _session.CloseAsync();
            _session.Dispose();
            _session = null;
        }
    }

    /// <summary>
    /// 按标签写入值
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WriteAsync<T>(
        string tag,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        EnsureConnected();
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var writeValue = new WriteValue
        {
            NodeId = new NodeId(tag),
            AttributeId = Attributes.Value,
            Value = new DataValue(new Variant(value)),
        };

        var response = await _session!.WriteAsync(null, [writeValue], cancellationToken);

        if (StatusCode.IsBad(response.Results[0]))
        {
            throw new InvalidOperationException($"写入标签 '{tag}' 失败: {response.Results[0]}");
        }
    }

    /// <summary>
    /// 按标签批量写入值
    /// </summary>
    /// <param name="tagValuePairs"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WriteMultipleAsync<T>(
        Dictionary<string, T> tagValuePairs,
        CancellationToken cancellationToken = default
    )
    {
        EnsureConnected();
        ArgumentNullException.ThrowIfNull(tagValuePairs);

        if (tagValuePairs.Count == 0)
        {
            return;
        }

        var writeValues = tagValuePairs
            .Select(kvp => new WriteValue
            {
                NodeId = new NodeId(kvp.Key),
                AttributeId = Attributes.Value,
                Value = new DataValue(new Variant(kvp.Value)),
            })
            .ToArray();

        var response = await _session!.WriteAsync(null, writeValues, cancellationToken);

        var failedTags = new List<string>();
        var tags = tagValuePairs.Keys.ToArray();

        for (var i = 0; i < response.Results.Count; i++)
        {
            if (!StatusCode.IsBad(response.Results[i]))
                continue;
            var tag = tags[i];
            failedTags.Add(tag);
        }

        if (failedTags.Count > 0)
        {
            throw new InvalidOperationException(
                $"以下标签写入失败: {string.Join(", ", failedTags)}"
            );
        }
    }

    /// <summary>
    /// 按标签读取值
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<T> ReadAsync<T>(string tag, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var readValue = new ReadValueId
        {
            NodeId = new NodeId(tag),
            AttributeId = Attributes.Value,
        };

        var response = await _session!.ReadAsync(
            null,
            0,
            TimestampsToReturn.Neither,
            [readValue],
            cancellationToken
        );

        var result = response.Results[0];
        if (StatusCode.IsBad(result.StatusCode))
        {
            throw new InvalidOperationException($"读取标签 '{tag}' 失败: {result.StatusCode}");
        }

        var convertedValue = ConvertValue<T>(result.Value);
        return convertedValue;
    }

    /// <summary>
    /// 按标签批量读取值
    /// </summary>
    /// <param name="tags"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Dictionary<string, object>> ReadMultipleAsync(
        string[] tags,
        CancellationToken cancellationToken = default
    )
    {
        EnsureConnected();
        ArgumentNullException.ThrowIfNull(tags);

        if (tags.Length == 0)
        {
            throw new ArgumentException("标签列表不能为空", nameof(tags));
        }

        var readValues = tags.Select(tag => new ReadValueId
            {
                NodeId = new NodeId(tag),
                AttributeId = Attributes.Value,
            })
            .ToArray();

        var response = await _session!.ReadAsync(
            null,
            0,
            TimestampsToReturn.Neither,
            readValues,
            cancellationToken
        );

        var result = new Dictionary<string, object>(tags.Length);

        for (var i = 0; i < response.Results.Count; i++)
        {
            var dataValue = response.Results[i];
            var tag = tags[i];

            if (StatusCode.IsGood(dataValue.StatusCode))
            {
                result[tag] = dataValue.Value;
            }
            else
            {
                result[tag] = "Error: " + dataValue.StatusCode;
            }
        }

        return result;
    }

    /// <summary>
    /// 注册数据变化处理器
    /// </summary>
    /// <param name="events"></param>
    public async Task RegisterDataChangeHandler(Event[] events)
    {
        EnsureConnected();
        ArgumentNullException.ThrowIfNull(events);

        _subscription = new Subscription(_session!.DefaultSubscription)
        {
            PublishingInterval = 100,
            PublishingEnabled = true,
        };

        var monitoredItems = events
            .Select(e => new MonitoredItem
            {
                StartNodeId = NodeId.Parse(e.Tag.Name),
                AttributeId = Attributes.Value,
                SamplingInterval = 100,
            })
            .ToList();

        foreach (var item in monitoredItems)
        {
            item.Notification += async (_, eventArgs) =>
            {
                if (eventArgs.NotificationValue is not MonitoredItemNotification notification)
                {
                    _logger.LogWarning("收到不支持的通知类型.");
                    return;
                }

                //检测数据是否bad
                if (StatusCode.IsBad(notification.Value.StatusCode))
                {
                    _logger.LogWarning(
                        "标签 {Tag} 数据状态异常: {StatusCode}",
                        item.StartNodeId,
                        notification.Value.StatusCode
                    );
                    return;
                }

                var value = notification.Value.WrappedValue.Value;
                var @event = events.FirstOrDefault(e => e.Tag.Name == item.StartNodeId.ToString());

                var log = @event?.TryCreateLog(value);
                if (log == null)
                    return;
                log.Parameters = new Dictionary<string, object>
                {
                    { "SourceTimestamp", notification.Value.SourceTimestamp },
                    { "ServerTimestamp", notification.Value.ServerTimestamp },
                    { "CurrentNodeId", item.StartNodeId },
                };
                //Console.WriteLine($"Event: {@event?.Name}, Value: {log.Value}, Timestamp: {log.Timestamp}");
                //await _uaRepository.AddEventLogAsync(log, CancellationToken.None);
                await _mediator.Publish(new EventLogCreatedEvent(log), CancellationToken.None);
            };
            _subscription.AddItem(item);
            _session!.AddSubscription(_subscription);
        }

        await _subscription.CreateAsync();
    }

    /// <summary>
    /// 注销数据变化处理器
    /// </summary>
    /// <param name="events"></param>
    public async Task UnregisterDataChangeHandler(Event[] events)
    {
        EnsureConnected();
        ArgumentNullException.ThrowIfNull(events);

        if (_subscription == null)
            return;

        var itemsToRemove = _subscription
            .MonitoredItems.Where(mi => events.Any(e => e.Tag.Name == mi.StartNodeId.ToString()))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            _subscription.RemoveItem(item);
        }

        if (!_subscription.MonitoredItems.Any())
        {
            await _session!.RemoveSubscriptionAsync(_subscription);
            await _subscription.DeleteAsync(true);
            _subscription = null;
        }
        else
        {
            await _subscription.ApplyChangesAsync();
        }
    }

    public ValueTask DisposeAsync() => DisconnectAsync();

    private void EnsureConnected()
    {
        if (_session?.Connected != true)
        {
            throw new InvalidOperationException("未连接到 OPC UA 服务器");
        }
    }

    private static T ConvertValue<T>(object? value)
    {
        try
        {
            return value switch
            {
                T directValue => directValue,
                null when default(T) is null => default!,
                null => throw new InvalidOperationException("无法将 null 转换为值类型"),
                _ => (T)Convert.ChangeType(value, typeof(T)),
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"类型转换失败，无法将 {value?.GetType().Name ?? "null"} 转换为 {typeof(T).Name}",
                ex
            );
        }
    }
}
