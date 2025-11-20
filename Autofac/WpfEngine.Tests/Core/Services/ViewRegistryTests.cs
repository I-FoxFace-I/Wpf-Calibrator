using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Windows;
using System.Windows.Controls;
using WpfEngine.Enums;
using WpfEngine.Services.Autofac;
using WpfEngine.ViewModels;
using WpfEngine.Views;
using Xunit;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for ViewRegistry - View mapping configuration
/// </summary>
public class ViewRegistryTests
{
    private readonly ViewRegistry _registry;
    private readonly Mock<ILogger<ViewRegistry>> _loggerMock;

    public ViewRegistryTests()
    {
        _loggerMock = new Mock<ILogger<ViewRegistry>>();
        _registry = new ViewRegistry(_loggerMock.Object);
    }

    [Fact]
    public void MapWindow_RegistersMapping()
    {
        // Act
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Assert
        _registry.TryGetViewType(typeof(TestViewModel), out var viewType).Should().BeTrue();
        viewType.Should().Be(typeof(TestWindow));
    }

    [Fact]
    public void MapDialog_RegistersMapping()
    {
        // Act
        _registry.MapDialog<TestViewModel, TestDialogWindow>();

        // Assert
        _registry.TryGetViewType(typeof(TestViewModel), out var viewType).Should().BeTrue();
        viewType.Should().Be(typeof(TestDialogWindow));
    }

    [Fact]
    public void MapControl_RegistersMapping()
    {
        // Act
        _registry.MapControl<TestViewModel, TestControl>();

        // Assert
        _registry.TryGetViewType(typeof(TestViewModel), out var viewType).Should().BeTrue();
        viewType.Should().Be(typeof(TestControl));
    }

    [Fact]
    public void MapWindow_OverwritesExistingMapping()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Act
        _registry.MapWindow<TestViewModel, TestWindow2>();

        // Assert
        _registry.TryGetViewType(typeof(TestViewModel), out var viewType).Should().BeTrue();
        viewType.Should().Be(typeof(TestWindow2));
    }

    [Fact]
    public void RemoveMapping_RemovesMapping()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Act
        _registry.RemoveMapping<TestViewModel>();

        // Assert
        _registry.TryGetViewType(typeof(TestViewModel), out _).Should().BeFalse();
    }

    [Fact]
    public void Clear_RemovesAllMappings()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();
        _registry.MapControl<TestViewModel2, TestControl>();

        // Act
        _registry.Clear();

        // Assert
        _registry.TryGetViewType(typeof(TestViewModel), out _).Should().BeFalse();
        _registry.TryGetViewType(typeof(TestViewModel2), out _).Should().BeFalse();
        _registry.GetAllMappings().Should().BeEmpty();
    }

    [Fact]
    public void GetAllMappings_ReturnsAllRegisteredMappings()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();
        _registry.MapControl<TestViewModel2, TestControl>();

        // Act
        var mappings = _registry.GetAllMappings();

        // Assert
        mappings.Should().HaveCount(2);
        mappings.Should().ContainKey(typeof(TestViewModel));
        mappings.Should().ContainKey(typeof(TestViewModel2));
    }

    // ========== TEST TYPES ==========

    public class TestViewModel : IViewModel
    {
        public Guid ViewModelId => Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }

        public bool HasError => throw new NotImplementedException();

        public string? ErrorMessage => throw new NotImplementedException();

        public Task InitializeAsync() => Task.CompletedTask;

        public void ClearError()
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public Task ReloadAsync()
        {
            throw new NotImplementedException();
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestViewModel2 : IViewModel
    {
        public Guid ViewModelId => Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }

        public bool HasError => throw new NotImplementedException();

        public string? ErrorMessage => throw new NotImplementedException();

        public Task InitializeAsync() => Task.CompletedTask;

        public void ClearError()
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public Task ReloadAsync()
        {
            throw new NotImplementedException();
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestWindow : Window, IWindowView
    {
        public Guid WindowId => Guid.NewGuid();
    }

    public class TestWindow2 : Window, IWindowView
    {
        public Guid WindowId => Guid.NewGuid();
    }

    public class TestDialogWindow : Window, IDialogView
    {
        public Guid WindowId => Guid.NewGuid();
        public DialogType DialogType => DialogType.Custom;
        public string? AppModule => "Test";
    }
    public class TestDialogWithParamsWindow : Window, IDialogView
    {
        public TestDialogWithParamsWindow()
        {
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public Guid WindowId => Guid.NewGuid();
        public DialogType DialogType => DialogType.Custom;
        public string? AppModule => "Test";

    }

    public class TestDialogWithResultWindow : Window, IDialogView
    {
        public TestDialogWithResultWindow()
        {
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public Guid WindowId => Guid.NewGuid();
        public DialogType DialogType => DialogType.Custom;
        public string? AppModule => "Test";
    }

    public class TestControl : UserControl, IControlView
    {
    }
}

