using FluentAssertions;
using Xunit;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Tests;

public sealed class AgentTaskTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var task = AgentTask.Create("Test Task", "Description", AgentId.New());
        task.Title.Should().Be("Test Task");
        task.TaskStatus.Should().Be(AgentTaskStatus.Pending);
    }

    [Fact]
    public void Start_ShouldTransitionToInProgress()
    {
        var task = AgentTask.Create("Test", "Desc", AgentId.New());
        task.Start();
        task.TaskStatus.Should().Be(AgentTaskStatus.InProgress);
    }

    [Fact]
    public void Complete_ShouldTransitionToCompleted()
    {
        var task = AgentTask.Create("Test", "Desc", AgentId.New());
        task.Complete();
        task.TaskStatus.Should().Be(AgentTaskStatus.Completed);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldTransitionToFailed()
    {
        var task = AgentTask.Create("Test", "Desc", AgentId.New());
        task.Fail();
        task.TaskStatus.Should().Be(AgentTaskStatus.Failed);
    }

    [Fact]
    public void Cancel_ShouldTransitionToCancelled()
    {
        var task = AgentTask.Create("Test", "Desc", AgentId.New());
        task.Cancel();
        task.TaskStatus.Should().Be(AgentTaskStatus.Cancelled);
    }
}
