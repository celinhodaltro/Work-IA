using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Board.Queries;

public sealed record ScheduleMeetingCommand(string Topic, List<Guid> ParticipantIds) : IRequest<Guid>;

public sealed class ScheduleMeetingHandler : IRequestHandler<ScheduleMeetingCommand, Guid>
{
    private readonly Work_IA.Domain.Ports.IMeetingRepository _repository;
    private readonly Work_IA.Application.Common.Interfaces.IUnitOfWork _unitOfWork;

    public ScheduleMeetingHandler(Work_IA.Domain.Ports.IMeetingRepository repository, Work_IA.Application.Common.Interfaces.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ScheduleMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = Work_IA.Domain.Agents.Meeting.Schedule(request.Topic, request.ParticipantIds);
        await _repository.AddAsync(meeting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return meeting.Id;
    }
}
