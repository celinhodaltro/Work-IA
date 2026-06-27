using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;

namespace Work_IA.Infrastructure.Persistence.Interceptors;

public sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher;
    private List<IDomainEvent>? _capturedEvents;

    public DispatchDomainEventsInterceptor(IDomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CaptureEvents(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureEvents(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (_capturedEvents is { Count: > 0 })
        {
            _dispatcher.DispatchAsync(_capturedEvents).GetAwaiter().GetResult();
            _capturedEvents = null;
        }

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_capturedEvents is { Count: > 0 })
        {
            await _dispatcher.DispatchAsync(_capturedEvents, cancellationToken);
            _capturedEvents = null;
        }

        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        RestoreEvents(eventData.Context);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        RestoreEvents(eventData.Context);
        await Task.CompletedTask;
    }

    private void CaptureEvents(DbContext? context)
    {
        if (context is null)
        {
            _capturedEvents = null;
            return;
        }

        var domainEvents = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        if (domainEvents.Count == 0)
        {
            _capturedEvents = null;
            return;
        }

        _capturedEvents = domainEvents;

        foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }

    private void RestoreEvents(DbContext? context)
    {
        if (_capturedEvents is null || context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
        {
            foreach (var domainEvent in _capturedEvents)
            {
                entry.Entity.GetType()
                    .GetMethod("RaiseDomainEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .Invoke(entry.Entity, [domainEvent]);
            }
        }

        _capturedEvents = null;
    }
}
