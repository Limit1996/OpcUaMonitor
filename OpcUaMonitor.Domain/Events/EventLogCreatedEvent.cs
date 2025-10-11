using MediatR;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Domain.Events;

public record EventLogCreatedEvent (EventLog EventLog): INotification;