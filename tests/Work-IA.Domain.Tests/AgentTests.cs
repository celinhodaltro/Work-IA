using FluentAssertions;
using Xunit;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Tests;

public sealed class AgentTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var agent = Agent.Create(new AgentName("TestAgent"), new AgentTitle("Tech Lead Backend"));
        agent.Name.Value.Should().Be("TestAgent");
        agent.Title.Value.Should().Be("Tech Lead Backend");
        agent.Status.Should().Be(AgentStatus.Created);
    }

    [Fact]
    public void Start_WhenCreated_ShouldTransitionToRunning()
    {
        var agent = Agent.Create(new AgentName("TestAgent"), new AgentTitle("Tech Lead Backend"));
        agent.Start();
        agent.Status.Should().Be(AgentStatus.Running);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_ShouldThrow()
    {
        var agent = Agent.Create(new AgentName("TestAgent"), new AgentTitle("Tech Lead Backend"));
        agent.Start();
        var act = () => agent.Start();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pause_WhenRunning_ShouldTransitionToPaused()
    {
        var agent = Agent.Create(new AgentName("TestAgent"), new AgentTitle("Tech Lead Backend"));
        agent.Start();
        agent.Pause();
        agent.Status.Should().Be(AgentStatus.Paused);
    }

    [Fact]
    public void AddObservationRule_ShouldAddRule()
    {
        var agent = Agent.Create(new AgentName("TestAgent"), new AgentTitle("Tech Lead Backend"));
        agent.AddObservationRule(new ObservationRule("FileModified", ObservationPriority.High));
        agent.ObservationRules.Should().HaveCount(1);
    }

    [Fact]
    public void ShouldObserve_WithMatchingRule_ShouldReturnTrue()
    {
        var agent = Agent.Create(new AgentName("TestAgent"), new AgentTitle("Tech Lead Backend"));
        agent.AddObservationRule(new ObservationRule("FileModified", ObservationPriority.High));
        agent.ShouldObserve("FileModified").Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => new AgentName("");
        act.Should().Throw<ArgumentException>();
    }
}
