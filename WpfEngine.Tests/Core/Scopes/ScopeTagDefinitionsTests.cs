using FluentAssertions;
using WpfEngine.Core.Scopes;
using Xunit;

namespace WpfEngine.Tests.Core.Scopes;

/// <summary>
/// Tests for ScopeTag type-safe tag system
/// </summary>
public class ScopeTagDefinitionsTests
{
    [Fact]
    public void ScopeTag_Root_CreatesRootTag()
    {
        // Act
        var tag = ScopeTag.Root();

        // Assert
        tag.Category.Should().Be(ScopeCategory.Root);
        tag.Name.Should().Be("root");
        tag.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ScopeTag_Window_CreatesWindowTagWithGeneratedGuid()
    {
        // Act
        var tag = ScopeTag.Window("TestWindow");

        // Assert
        tag.Category.Should().Be(ScopeCategory.Window);
        tag.Name.Should().Be("TestWindow");
        tag.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ScopeTag_Window_WithProvidedGuid_UsesProvidedGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();

        // Act
        var tag = ScopeTag.Window("TestWindow", expectedGuid);

        // Assert
        tag.Id.Should().Be(expectedGuid);
    }

    [Fact]
    public void ScopeTag_WorkflowSession_CreatesWorkflowSessionTag()
    {
        // Act
        var tag = ScopeTag.WorkflowSession("order-workflow");

        // Assert
        tag.Category.Should().Be(ScopeCategory.WorkflowSession);
        tag.Name.Should().Be("order-workflow");
        tag.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ScopeTag_ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var tag = ScopeTag.Window("TestWindow", guid);

        // Act
        var result = tag.ToString();

        // Assert
        result.Should().Be($"Window:TestWindow:{guid}");
    }

    [Fact]
    public void ScopeTag_ToShortString_ReturnsNameAndCategory()
    {
        // Arrange
        var tag = ScopeTag.WorkflowSession("order-workflow");

        // Act
        var result = tag.ToShortString();

        // Assert
        result.Should().Be("WorkflowSession:order-workflow");
    }

    [Fact]
    public void ScopeTag_Equals_ReturnsTrueForSameValues()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var tag1 = ScopeTag.Window("Test", guid);
        var tag2 = ScopeTag.Window("Test", guid);

        // Act & Assert
        tag1.Should().Be(tag2);
        (tag1 == tag2).Should().BeTrue();
    }

    [Fact]
    public void ScopeTag_Equals_ReturnsFalseForDifferentValues()
    {
        // Arrange
        var tag1 = ScopeTag.Window("Test1");
        var tag2 = ScopeTag.Window("Test2");

        // Act & Assert
        tag1.Should().NotBe(tag2);
        (tag1 != tag2).Should().BeTrue();
    }

    [Theory]
    [InlineData(ScopeCategory.Window, true)]
    [InlineData(ScopeCategory.WorkflowSession, false)]
    [InlineData(ScopeCategory.Dialog, false)]
    public void ScopeTagExtensions_IsWindow_ReturnsCorrectValue(ScopeCategory category, bool expected)
    {
        // Arrange
        var tag = category switch
        {
            ScopeCategory.Window => ScopeTag.Window("Test"),
            ScopeCategory.WorkflowSession => ScopeTag.WorkflowSession("Test"),
            ScopeCategory.Dialog => ScopeTag.Dialog("Test"),
            _ => ScopeTag.Custom("Test")
        };

        // Act
        var result = tag.IsWindow();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ScopeCategory.WorkflowSession, true)]
    [InlineData(ScopeCategory.Window, false)]
    [InlineData(ScopeCategory.Dialog, false)]
    public void ScopeTagExtensions_IsWorkflowSession_ReturnsCorrectValue(ScopeCategory category, bool expected)
    {
        // Arrange
        var tag = category switch
        {
            ScopeCategory.Window => ScopeTag.Window("Test"),
            ScopeCategory.WorkflowSession => ScopeTag.WorkflowSession("Test"),
            ScopeCategory.Dialog => ScopeTag.Dialog("Test"),
            _ => ScopeTag.Custom("Test")
        };

        // Act
        var result = tag.IsWorkflowSession();

        // Assert
        result.Should().Be(expected);
    }
}

