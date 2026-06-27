using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Communication;

namespace Work_IA.Infrastructure.Communication;

public sealed class CommunicationBus : ICommunicationBus, IDisposable
{
    private readonly AgentRegistry _registry;
    private readonly ILogger<CommunicationBus> _logger;
    private readonly ConcurrentDictionary<ConversationRoomId, ConversationRoom> _rooms = new();
    private readonly Subject<AgentMessage> _messageStream = new();
    private readonly IObservable<AgentMessage> _synchronizedStream;
    private bool _disposed;

    public IObservable<AgentMessage> MessageStream => _synchronizedStream;

    public CommunicationBus(AgentRegistry registry, ILogger<CommunicationBus> logger)
    {
        _registry = registry;
        _logger = logger;
        _synchronizedStream = _messageStream.Synchronize();
    }

    public async Task SendDirectMessageAsync(AgentId from, AgentId to, string content, MessageType type = MessageType.Direct, CancellationToken cancellationToken = default)
    {
        var message = new AgentMessage(from, to, content, type);

        var targetAgent = _registry.Get(to);
        if (targetAgent is not null)
        {
            await targetAgent.HandleMessageAsync(message, cancellationToken);
        }

        _messageStream.OnNext(message);
        _logger.LogInformation("Message sent from {From} to {To}", from, to);
    }

    public async Task BroadcastAsync(AgentId from, string content, AgentCareerLevel? targetLevel = null, CancellationToken cancellationToken = default)
    {
        var targets = targetLevel.HasValue
            ? _registry.GetByCareerLevel(targetLevel.Value)
            : _registry.GetAll();

        foreach (var target in targets)
        {
            if (target.AgentId != from)
            {
                var message = new AgentMessage(from, target.AgentId, content, MessageType.Broadcast);
                await target.HandleMessageAsync(message, cancellationToken);
                _messageStream.OnNext(message);
            }
        }

        _logger.LogInformation("Broadcast from {From} to level {Level}", from, targetLevel);
    }

    public Task<ConversationRoom> CreateRoomAsync(string topic, List<AgentId> participants, CancellationToken cancellationToken = default)
    {
        var room = ConversationRoom.Create(topic, participants);
        _rooms[room.Id] = room;
        _logger.LogInformation("Room created: {Topic}", topic);
        return Task.FromResult(room);
    }

    public async Task JoinRoomAsync(AgentId agentId, ConversationRoomId roomId, CancellationToken cancellationToken = default)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.AddParticipant(agentId);
            _logger.LogInformation("Agent {AgentId} joined room {RoomId}", agentId, roomId);
        }
    }

    public async Task SendToRoomAsync(ConversationRoomId roomId, AgentId from, string content, CancellationToken cancellationToken = default)
    {
        if (!_rooms.TryGetValue(roomId, out var room))
            return;

        foreach (var participant in room.Participants)
        {
            if (participant != from)
            {
                var message = new AgentMessage(from, participant, content, MessageType.Direct);
                room.AddMessage(message);
                _messageStream.OnNext(message);

                var agent = _registry.Get(participant);
                if (agent is not null)
                {
                    await agent.HandleMessageAsync(message, cancellationToken);
                }
            }
        }
    }

    public Task<ConversationRoom?> GetRoomAsync(ConversationRoomId roomId, CancellationToken cancellationToken = default)
    {
        _rooms.TryGetValue(roomId, out var room);
        return Task.FromResult(room);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _messageStream.Dispose();
        _disposed = true;
    }
}
