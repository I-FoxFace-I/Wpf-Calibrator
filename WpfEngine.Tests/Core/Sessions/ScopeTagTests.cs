using FluentAssertions;
using WpfEngine.Data.Sessions;
using Xunit;

namespace WpfEngine.Tests.Core.Sessions;

public class ScopeTagTests
{
    [Fact]
    public void Root_ShouldCreateRootTag()
    {
        // Act
        var tag = ScopeTag.Root();
        
        // Assert
        tag.Category.Should().Be(ScopeCategory.Root);
        tag.Name.Should().BeNull();
        tag.ToString().Should().Be("Root");
    }
    
    [Fact]
    public void Window_ShouldCreateWindowTag()
    {
        // Act
        var tag = ScopeTag.Window();
        
        // Assert
        tag.Category.Should().Be(ScopeCategory.Window);
        tag.Name.Should().BeNull();
        tag.ToString().Should().Be("Window");
    }
    
    [Fact]
    public void Database_WithoutName_ShouldCreateDatabaseTag()
    {
        // Act
        var tag = ScopeTag.Database();
        
        // Assert
        tag.Category.Should().Be(ScopeCategory.Database);
        tag.Name.Should().BeNull();
        tag.ToString().Should().Be("Database");
    }
    
    [Fact]
    public void Database_WithName_ShouldIncludeNameInTag()
    {
        // Act
        var tag = ScopeTag.Database("AppContext");
        
        // Assert
        tag.Category.Should().Be(ScopeCategory.Database);
        tag.Name.Should().Be("AppContext");
        tag.ToString().Should().Be("Database:AppContext");
    }
    
    [Fact]
    public void Workflow_WithName_ShouldCreateWorkflowTag()
    {
        // Act
        var tag = ScopeTag.Workflow("OrderEntry");
        
        // Assert
        tag.Category.Should().Be(ScopeCategory.Workflow);
        tag.Name.Should().Be("OrderEntry");
        tag.ToString().Should().Be("Workflow:OrderEntry");
    }
    
    [Fact]
    public void Workflow_WithoutName_ShouldCreateWorkflowTag()
    {
        // Act
        var tag = ScopeTag.Workflow();
        
        // Assert
        tag.Category.Should().Be(ScopeCategory.Workflow);
        tag.Name.Should().BeNull();
        tag.ToString().Should().Be("Workflow");
    }
    
    [Fact]
    public void Custom_ShouldRequireName()
    {
        // Act
        var action = () => ScopeTag.Custom(null!);
        
        // Assert
        action.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void Custom_WithName_ShouldCreateCustomTag()
    {
        // Act
        var tag = ScopeTag.Custom("MyCustomScope");
        
        // Assert
        tag.Category.Should().Be(ScopeCategory.Custom);
        tag.Name.Should().Be("MyCustomScope");
        tag.ToString().Should().Be("Custom:MyCustomScope");
    }
    
    [Theory]
    [InlineData("Database", null, "Database")]
    [InlineData("Database", "AppContext", "Database:AppContext")]
    [InlineData("Workflow", "OrderEntry", "Workflow:OrderEntry")]
    public void ToAutofacTag_ForDatabaseAndWorkflow_ShouldShareByName(
        string category, 
        string? name, 
        string expected)
    {
        // Arrange
        var scopeCategory = Enum.Parse<ScopeCategory>(category);
        var tag = new ScopeTag(scopeCategory, name);
        
        // Act
        var autofacTag = tag.ToAutofacTag();
        
        // Assert
        autofacTag.Should().Be(expected);
    }
    
    [Fact]
    public void ToAutofacTag_ForWindow_ShouldUseCategory()
    {
        // Arrange
        var tag = ScopeTag.Window();
        
        // Act
        var autofacTag = tag.ToAutofacTag();
        
        // Assert
        autofacTag.Should().Be("Window");
    }
    
    [Fact]
    public void Equals_SameCategoryAndName_ShouldBeEqual()
    {
        // Arrange
        var tag1 = ScopeTag.Database("AppContext");
        var tag2 = ScopeTag.Database("AppContext");
        
        // Act & Assert
        tag1.Should().Be(tag2);
        (tag1 == tag2).Should().BeTrue();
        tag1.GetHashCode().Should().Be(tag2.GetHashCode());
    }
    
    [Fact]
    public void Equals_DifferentNames_ShouldNotBeEqual()
    {
        // Arrange
        var tag1 = ScopeTag.Database("AppContext");
        var tag2 = ScopeTag.Database("OtherContext");
        
        // Act & Assert
        tag1.Should().NotBe(tag2);
        (tag1 != tag2).Should().BeTrue();
    }
    
    [Fact]
    public void IsDatabase_ForDatabaseTag_ShouldReturnTrue()
    {
        // Arrange
        var tag = ScopeTag.Database();
        
        // Act & Assert
        tag.IsDatabase().Should().BeTrue();
        tag.IsWorkflow().Should().BeFalse();
        tag.IsWindow().Should().BeFalse();
    }
    
    [Fact]
    public void IsWorkflow_ForWorkflowTag_ShouldReturnTrue()
    {
        // Arrange
        var tag = ScopeTag.Workflow("Test");
        
        // Act & Assert
        tag.IsWorkflow().Should().BeTrue();
        tag.IsDatabase().Should().BeFalse();
        tag.IsWindow().Should().BeFalse();
    }
    
    [Fact]
    public void IsCustom_ForCustomTag_ShouldReturnTrue()
    {
        // Arrange
        var tag = ScopeTag.Custom("MyCustom");
        
        // Act & Assert
        tag.IsCustom().Should().BeTrue();
        tag.IsDatabase().Should().BeFalse();
    }
}

