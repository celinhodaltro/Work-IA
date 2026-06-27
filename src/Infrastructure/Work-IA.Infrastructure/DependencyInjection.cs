using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Infrastructure.EventBus;
using Work_IA.Infrastructure.Persistence;
using Work_IA.Infrastructure.Persistence.EventStore;
using Work_IA.Infrastructure.Persistence.Interceptors;
using Work_IA.Infrastructure.Services;

namespace Work_IA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<WorkIaDbContext>((sp, options) =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(WorkIaDbContext).Assembly.FullName))
                .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>()));

        services.AddSingleton<IEventTypeResolver>(sp =>
        {
            var domainAssembly = typeof(Work_IA.Domain.Abstractions.IDomainEvent).Assembly;
            var types = domainAssembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IDomainEvent)) && !t.IsAbstract && !t.IsInterface)
                .ToList();
            return new EventTypeResolver(types);
        });

        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        return services;
    }
}
