using MediatR;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Queries;

public sealed record GetMemoriesByTypeQuery(MemoryType Type, int Limit = 10) : IRequest<IReadOnlyList<MemoryEntry>>;
