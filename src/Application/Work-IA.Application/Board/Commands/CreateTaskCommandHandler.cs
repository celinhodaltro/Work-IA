using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Ports;

namespace Work_IA.Application.Board.Commands;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly IBoardRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(IBoardRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = BoardTask.Create(request.Title, request.Description, request.Priority);
        await _repository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return task.Id;
    }
}
