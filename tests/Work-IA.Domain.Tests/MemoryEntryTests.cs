using FluentAssertions;
using Xunit;
using Work_IA.Domain.Memory;

namespace Work_IA.Domain.Tests;

public sealed class MemoryEntryTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var entry = MemoryEntry.Create(MemoryType.Success, "Title", "Content", ["tag1"], 10);
        entry.Title.Should().Be("Title");
        entry.Type.Should().Be(MemoryType.Success);
        entry.Tags.Should().Contain("tag1");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrow()
    {
        var act = () => MemoryEntry.Create(MemoryType.Success, "", "Content");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateScore_WithInvalidScore_ShouldThrow()
    {
        var entry = MemoryEntry.Create(MemoryType.Success, "Title", "Content");
        var act = () => entry.UpdateScore(15);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddTag_ShouldAddNewTag()
    {
        var entry = MemoryEntry.Create(MemoryType.Success, "Title", "Content");
        entry.AddTag("newTag");
        entry.Tags.Should().Contain("newTag");
    }
}
