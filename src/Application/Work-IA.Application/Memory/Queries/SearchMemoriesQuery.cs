using MediatR;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Queries;

public sealed record SearchMemoriesQuery(string Query, int Limit = 10) : IRequest<IReadOnlyList<MemoryEntry>>;
