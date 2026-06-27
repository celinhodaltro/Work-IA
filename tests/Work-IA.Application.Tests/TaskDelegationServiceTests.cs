using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Work_IA.Application.Agents;
using Work_IA.Application.Agents.Commands;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Tests;

public sealed class DelegateTaskCommandTests
{
    [Fact]
    public async Task DelegateCommand_WithNoAgent_ShouldReturnFail()
    {
        var registry = new AgentRegistry();
        var handler = new DelegateTaskCommandHandler(registry);
        var command = new DelegateTaskCommand("Test", "Desc", AgentCareerLevel.Architect, TaskPriority.Normal);
        var result = await handler.Handle(command, CancellationToken.None);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DelegateCommand_WithAvailableAgent_ShouldReturnSuccess()
    {
        var registry = new AgentRegistry();
        var eventBus = new Mock<IEventBus>();
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<AgentBase>>();
        var agent = new TestAgent(eventBus.Object, mediator.Object, logger.Object);
        await agent.InitializeAsync();
        registry.Register(agent);
        var handler = new DelegateTaskCommandHandler(registry);
        var command = new DelegateTaskCommand("Test", "Desc", AgentCareerLevel.Intern, TaskPriority.Normal);
        var result = await handler.Handle(command, CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    public sealed class TestAgent : AgentBase
    {
        public TestAgent(IEventBus eventBus, IMediator mediator, ILogger<AgentBase> logger)
            : base(Agent.Create(new AgentName("TestAgent"), new AgentTitle("Tech Lead Backend")), eventBus, mediator, logger) { }

        protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken ct)
        {
            return Task.FromResult(TaskResult.Ok("done"));
        }
    }
}
