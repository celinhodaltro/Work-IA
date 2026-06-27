using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Application.Services;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Tests;

public sealed class TaskDelegationServiceTests
{
    [Fact]
    public async Task DelegateAsync_WithNoAgent_ShouldReturnFail()
    {
        var registry = new AgentRegistry();
        var eventBus = new Mock<IEventBus>();
        var service = new TaskDelegationService(registry, eventBus.Object);
        var result = await service.DelegateAsync("Test", "Desc", AgentRole.Architect);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DelegateAsync_WithAvailableAgent_ShouldReturnSuccess()
    {
        var registry = new AgentRegistry();
        var eventBus = new Mock<IEventBus>();
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<AgentBase>>();
        var agent = new TestAgent(eventBus.Object, mediator.Object, logger.Object);
        await agent.InitializeAsync();
        registry.Register(agent);
        var service = new TaskDelegationService(registry, eventBus.Object);
        var result = await service.DelegateAsync("Test", "Desc", AgentRole.TechLeadBackend);
        result.Success.Should().BeTrue();
    }

    public sealed class TestAgent : AgentBase
    {
        public TestAgent(IEventBus eventBus, IMediator mediator, ILogger<AgentBase> logger)
            : base("TestAgent", AgentRole.TechLeadBackend, eventBus, mediator, logger) { }

        protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken ct)
        {
            return Task.FromResult(TaskResult.Ok("done"));
        }
    }
}
