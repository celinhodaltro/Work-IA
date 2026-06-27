using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Work_IA.Application.Agents;
using Work_IA.Application.Behaviors;
using Work_IA.Application.Services;

namespace Work_IA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        services.AddSingleton<AgentRegistry>();
        services.AddTransient<TaskDelegationService>();
        services.AddScoped<MemoryRetrievalService>();
        services.AddScoped<LearningEngine>();
        services.AddSingleton<AgentRoleFactory>();
        services.AddScoped<RoomSummaryService>();

        return services;
    }
}
