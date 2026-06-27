using MediatR;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Queries;

public sealed record GetMemoriesByTagsQuery(string[] Tags, int Limit = 10) : IRequest<IReadOnlyList<MemoryEntry>>;
