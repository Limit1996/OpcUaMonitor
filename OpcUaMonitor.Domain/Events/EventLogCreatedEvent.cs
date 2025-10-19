using MediatR;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Events;

public record EventLogCreatedEvent (EventLog EventLog): INotification;
public record ConnectionLostEvent (Channel Channel): INotification;