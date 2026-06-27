using FluentAssertions;
using Moq;
using Xunit;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Tests;

public sealed class AgentRegistryTests
{
    [Fact]
    public void Register_ShouldAddAgent()
    {
        var registry = new AgentRegistry();
        var agent = new Mock<IAgent>();
        agent.Setup(a => a.AgentId).Returns(AgentId.New());
        registry.Register(agent.Object);
        registry.Count.Should().Be(1);
    }

    [Fact]
    public void Get_WithValidId_ShouldReturnAgent()
    {
        var registry = new AgentRegistry();
        var agent = new Mock<IAgent>();
        var id = AgentId.New();
        agent.Setup(a => a.AgentId).Returns(id);
        registry.Register(agent.Object);
        registry.Get(id).Should().Be(agent.Object);
    }

    [Fact]
    public void GetByCareerLevel_ShouldFilterCorrectly()
    {
        var registry = new AgentRegistry();
        var agent1 = new Mock<IAgent>();
        agent1.Setup(a => a.AgentId).Returns(AgentId.New());
        agent1.Setup(a => a.CareerLevel).Returns(AgentCareerLevel.Architect);
        var agent2 = new Mock<IAgent>();
        agent2.Setup(a => a.AgentId).Returns(AgentId.New());
        agent2.Setup(a => a.CareerLevel).Returns(AgentCareerLevel.TechLead);
        registry.Register(agent1.Object);
        registry.Register(agent2.Object);
        registry.GetByCareerLevel(AgentCareerLevel.Architect).Should().HaveCount(1);
    }
}
