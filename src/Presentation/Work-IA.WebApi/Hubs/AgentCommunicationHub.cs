using Microsoft.AspNetCore.SignalR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Communication;

namespace Work_IA.WebApi.Hubs;

public sealed class AgentCommunicationHub : Hub
{
    private readonly ICommunicationBus _bus;

    public AgentCommunicationHub(ICommunicationBus bus)
    {
        _bus = bus;
    }

    public async Task SendDirectMessage(string fromId, string toId, string content)
    {
        var from = AgentId.From(Guid.Parse(fromId));
        var to = AgentId.From(Guid.Parse(toId));

        await _bus.SendDirectMessageAsync(from, to, content);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SendToRoom(string roomId, string fromId, string content)
    {
        var from = AgentId.From(Guid.Parse(fromId));
        await _bus.SendToRoomAsync(ConversationRoomId.From(Guid.Parse(roomId)), from, content);
    }
}
