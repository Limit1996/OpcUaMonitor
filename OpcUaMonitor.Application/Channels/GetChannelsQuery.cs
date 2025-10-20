using MediatR;
using OpcUaMonitor.Domain.Abstractions;

namespace OpcUaMonitor.Application.Channels;

public record GetChannelsQuery(Guid SystemDeviceId) : IRequest<Result<List<ChannelResponse>>>
{
    
}