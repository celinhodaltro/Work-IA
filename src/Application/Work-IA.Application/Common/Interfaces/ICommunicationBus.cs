using Work_IA.Domain.Agents;
using Work_IA.Domain.Communication;

namespace Work_IA.Application.Common.Interfaces;

public interface ICommunicationBus
{
    Task SendDirectMessageAsync(AgentId from, AgentId to, string content, MessageType type = MessageType.Direct, CancellationToken cancellationToken = default);
    Task BroadcastAsync(AgentId from, string content, AgentRole? targetRole = null, CancellationToken cancellationToken = default);
    Task<ConversationRoom> CreateRoomAsync(string topic, List<AgentId> participants, CancellationToken cancellationToken = default);
    Task JoinRoomAsync(AgentId agentId, ConversationRoomId roomId, CancellationToken cancellationToken = default);
    Task SendToRoomAsync(ConversationRoomId roomId, AgentId from, string content, CancellationToken cancellationToken = default);
    Task<ConversationRoom?> GetRoomAsync(ConversationRoomId roomId, CancellationToken cancellationToken = default);
    IObservable<AgentMessage> MessageStream { get; }
}
