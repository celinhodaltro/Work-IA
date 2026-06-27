using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Communication;
using Microsoft.Extensions.Logging;

namespace Work_IA.Application.Services;

public sealed class RoomSummaryService
{
    private readonly ICommunicationBus _bus;
    private readonly ILogger<RoomSummaryService> _logger;

    public RoomSummaryService(ICommunicationBus bus, ILogger<RoomSummaryService> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task<string> GenerateSummaryAsync(ConversationRoomId roomId, CancellationToken cancellationToken = default)
    {
        var room = await _bus.GetRoomAsync(roomId, cancellationToken);

        if (room is null)
            return string.Empty;

        var summary = $"Room: {room.Topic}\n" +
                      $"Participants: {room.Participants.Count}\n" +
                      $"Messages: {room.Messages.Count}\n" +
                      $"Status: {room.Status}";

        return summary;
    }
}
