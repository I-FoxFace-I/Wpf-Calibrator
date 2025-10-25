# WpfEngine

**A modern WPF MVVM framework with hierarchical dependency injection, type-safe scope management, and advanced window lifecycle control.**

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Core Concepts](#core-concepts)
- [Getting Started](#getting-started)
- [Usage Examples](#usage-examples)
- [Advanced Scenarios](#advanced-scenarios)
- [Best Practices](#best-practices)
- [API Reference](#api-reference)

---

## Overview

WpfEngine is a comprehensive WPF framework built on top of **Autofac** dependency injection that provides:

- **Hierarchical Scope Management**: Root → Session → Window scopes with proper lifetime control
- **Type-Safe Scope Tags**: Strongly-typed scope identification using `ScopeTag` structs
- **Advanced Window Management**: Parent-child relationships, session-based windows, weak reference tracking
- **MVVM Infrastructure**: Base classes for ViewModels, Views, and comprehensive navigation services
- **Shared Service Support**: `InstancePerMatchingLifetimeScope` for sharing services across related windows
- **Content Navigation**: Shell-based content management with history support

---

## Key Features

### 🎯 **Hierarchical Scope System**

```
Root Container
  ├── Session Scope ("workflow-session")
  │   ├── Window Scope (Window1)
  │   ├── Window Scope (Window2)
  │   └── Window Scope (Window3)
  └── Window Scope (Standalone Window)
```

### 🏷️ **Type-Safe Scope Tags**

```csharp
public enum ScopeCategory
{
    Root,
    Window,
    WorkflowSession,
    Dialog,
    Custom
}

var scopeTag = ScopeTag.WorkflowSession("OrderWorkflow", sessionId);
```

### 🪟 **Advanced Window Management**

- Session-based window grouping
- Parent-child window relationships
- Automatic cleanup with weak references
- Multiple closing strategies (by ID, VmKey, Window instance)

### 🔄 **Service Sharing**

Services registered with `InstancePerMatchingLifetimeScope` can be shared across:
- All windows in a workflow session
- Parent and child windows
- Shell and its content

---

## Architecture

### Dependency Injection Layers

```
┌─────────────────────────────────────────────────┐
│          Root Container (Autofac)                │
│  - Singleton services                            │
│  - IWindowService, IDialogService               │
└────────────────┬────────────────────────────────┘
                 │
      ┌──────────┴──────────┐
      │                      │
┌─────▼──────────┐   ┌──────▼────────────────────┐
│ Session Scope  │   │  Window Scope             │
│ (Optional)     │   │  - Window-specific        │
│ - Shared       │   │    services               │
│   services     │   │  - IContentManager        │
│   across       │   │  - Window lifetime        │
│   workflow     │   └───────────────────────────┘
└────────┬───────┘
         │
    ┌────┴────┐
    │         │
┌───▼──┐  ┌──▼───┐
│Window│  │Window│
│Scope │  │Scope │
└──────┘  └──────┘
```

### Core Services

| Service | Lifetime | Purpose |
|---------|----------|---------|
| `ViewRegistry` | Singleton | Maps ViewModels to Views |
| `ViewLocatorService` | PerLifetimeScope | Resolves Views from mappings |
| `ViewModelFactory` | PerLifetimeScope | Creates ViewModels with parameters |
| `NavigationService` | PerLifetimeScope | Content navigation with history |
| `ContentManager` | PerMatchingLifetimeScope (Window) | Shell content management |
| `WindowService` | PerLifetimeScope | Window lifecycle management |
| `DialogService` | PerLifetimeScope | Modal dialog management |

---

## Core Concepts

### 1. ViewModels

All ViewModels inherit from base classes:

```csharp
// Simple ViewModel
public class MyViewModel : BaseViewModel
{
    public MyViewModel(ILogger<MyViewModel> logger) : base(logger) { }
    
    public override Task InitializeAsync()
    {
        // Async initialization
        return Task.CompletedTask;
    }
}

// ViewModel with Parameters
public class CustomerDetailViewModel : BaseViewModel<CustomerDetailParams>
{
    public CustomerDetailViewModel(ILogger<CustomerDetailViewModel> logger) 
        : base(logger) { }
    
    public override async Task InitializeAsync(CustomerDetailParams parameters)
    {
        // Load customer by parameters.CustomerId
        await LoadCustomerAsync(parameters.CustomerId);
    }
}

// Shell ViewModel (for windows with content navigation)
public class WorkflowHostViewModel : ShellViewModel
{
    public WorkflowHostViewModel(
        IContentManager contentManager,
        IWindowService windowService,
        ILogger<WorkflowHostViewModel> logger) 
        : base(contentManager, windowService, logger) { }
    
    public override async Task InitializeAsync()
    {
        // Navigate to first step
        await ContentManager.NavigateToAsync<Step1ViewModel>();
    }
}
```

### 2. Views

Views must implement `IWindowView`, `IDialogView`, or `IControlView`:

```csharp
// Window View
public partial class CustomerListWindow : Window, IWindowView
{
    public CustomerListWindow(ILogger<CustomerListWindow> logger)
    {
        InitializeComponent();
        Logger = logger;
    }
    
    public Guid WindowId { get; } = Guid.NewGuid();
    
    object? IView.DataContext
    {
        get => DataContext;
        set => DataContext = value;
    }
}

// Shell Window with ContentControl
public partial class WorkflowHostWindow : Window, IShellView
{
    public WorkflowHostWindow(ILogger<WorkflowHostWindow> logger)
    {
        InitializeComponent();
    }
    
    public Guid WindowId { get; } = Guid.NewGuid();
    
    object? IView.DataContext
    {
        get => DataContext;
        set => DataContext = value;
    }
}
```

**XAML for Shell:**

```xml
<Window x:Class="MyApp.Views.WorkflowHostWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        Title="Workflow">
    <ContentControl Content="{Binding CurrentContent}">
        <ContentControl.Resources>
            <DataTemplate DataType="{x:Type vm:Step1ViewModel}">
                <views:Step1View />
            </DataTemplate>
            <DataTemplate DataType="{x:Type vm:Step2ViewModel}">
                <views:Step2View />
            </DataTemplate>
        </ContentControl.Resources>
    </ContentControl>
</Window>
```

### 3. View Mapping Configuration

```csharp
public class MyViewMappingConfiguration : ViewMappingConfiguration
{
    public override void Configure(IViewRegistry registry)
    {
        // Windows
        registry.MapWindow<CustomerListViewModel, CustomerListWindow>();
        registry.MapWindow<ProductListViewModel, ProductListWindow>();
        
        // Dialogs
        registry.MapDialog<CustomerDetailViewModel, CustomerDetailWindow>();
        
        // Shells
        registry.MapShell<WorkflowHostViewModel, WorkflowHostWindow>();
        
        // UserControls (for content navigation)
        registry.MapControl<Step1ViewModel, Step1View>();
        registry.MapControl<Step2ViewModel, Step2View>();
        registry.MapControl<Step3ViewModel, Step3View>();
    }
}
```

### 4. Autofac Module Registration

```csharp
public class MyAppModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register ViewModels
        builder.RegisterType<CustomerListViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<CustomerDetailViewModel>().AsSelf().InstancePerDependency();
        
        // Register Views
        builder.RegisterType<CustomerListWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<CustomerDetailWindow>().AsSelf().InstancePerDependency();
        
        // Register shared services for workflow sessions
        builder.RegisterType<OrderBuilderService>()
               .As<IOrderBuilderService>()
               .InstancePerMatchingLifetimeScope("workflow-session");
        
        // Register view mappings
        builder.RegisterType<MyViewMappingConfiguration>()
               .As<ViewMappingConfiguration>()
               .SingleInstance();
    }
}
```

---

## Getting Started

### 1. Application Startup (App.xaml.cs)

```csharp
public partial class App : Application
{
    private IContainer? _container;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Build Autofac container
        var builder = new ContainerBuilder();
        
        // Register logging
        builder.RegisterInstance(LoggerFactory.Create(config => 
        {
            config.AddConsole();
            config.AddDebug();
        })).As<ILoggerFactory>().SingleInstance();
        
        builder.RegisterGeneric(typeof(Logger<>))
               .As(typeof(ILogger<>))
               .SingleInstance();
        
        // Register Core services
        builder.RegisterModule<CoreServicesModule>();
        
        // Register your application modules
        builder.RegisterModule<MyAppModule>();
        
        _container = builder.Build();
        
        // Configure view mappings
        var registry = _container.Resolve<IViewRegistry>();
        var mappingConfigs = _container.Resolve<IEnumerable<ViewMappingConfiguration>>();
        foreach (var config in mappingConfigs)
        {
            config.Configure(registry);
        }
        
        // Open main window
        var windowService = _container.Resolve<IWindowService>();
        windowService.OpenWindow<MainViewModel>();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        _container?.Dispose();
        base.OnExit(e);
    }
}
```

### 2. Opening Windows

```csharp
// Simple window
_windowService.OpenWindow<CustomerListViewModel>();

// Window with parameters
var parameters = new CustomerDetailParams { CustomerId = 123 };
_windowService.OpenWindow<CustomerDetailViewModel, CustomerDetailParams>(parameters);

// Child window (attached to parent)
var childId = _windowService.OpenChildWindow<ProductSelectorViewModel>(parentWindowId);
```

### 3. Closing Windows

```csharp
// By window ID
_windowService.Close(windowId);

// By VmKey (from ViewModel)
_windowService.Close(this.GetVmKey());

// By Window instance
_windowService.Close(window);

// Close all children
_windowService.CloseAllChildWindows(parentWindowId);
```

### 4. Content Navigation (in Shell ViewModels)

```csharp
// Navigate forward
await ContentManager.NavigateToAsync<Step2ViewModel>();

// Navigate with parameters
var options = new Step2Params { Data = "something" };
await ContentManager.NavigateToAsync<Step2ViewModel, Step2Params>(options);

// Navigate back
if (ContentManager.CanNavigateBack)
{
    await ContentManager.NavigateBackAsync();
}

// Clear history
ContentManager.ClearHistory();

// Request shell close from content
ContentManager.RequestShellClose(showConfirmation: true, "Save changes?");
```

---

## Usage Examples

### Example 1: Simple Window Opening

```csharp
public class MainViewModel : BaseViewModel
{
    private readonly IWindowService _windowService;
    
    public MainViewModel(
        IWindowService windowService,
        ILogger<MainViewModel> logger) : base(logger)
    {
        _windowService = windowService;
    }
    
    [RelayCommand]
    private void OpenCustomerList()
    {
        _windowService.OpenWindow<CustomerListViewModel>();
    }
}
```

### Example 2: Detail Window with Parameters

```csharp
public class CustomerListViewModel : BaseViewModel
{
    private readonly IWindowService _windowService;
    
    [ObservableProperty]
    private CustomerDto? _selectedCustomer;
    
    [RelayCommand]
    private void ViewCustomerDetail()
    {
        if (SelectedCustomer == null) return;
        
        var parameters = new CustomerDetailParams 
        { 
            CustomerId = SelectedCustomer.Id 
        };
        
        _windowService.OpenWindow<CustomerDetailViewModel, CustomerDetailParams>(parameters);
    }
}
```

### Example 3: Workflow with Session

```csharp
// 1. Create workflow session interface
public interface IWorkflowSession
{
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;
    Guid OpenWindow<TViewModel, TOptions>(TOptions options) 
        where TViewModel : IViewModel 
        where TOptions : IVmParameters;
    void CloseSession();
}

// 2. Implementation
public class WorkflowSession : IWorkflowSession
{
    private readonly IWindowService _windowService;
    private readonly Guid _sessionId;
    
    public WorkflowSession(IWindowService windowService, Guid sessionId)
    {
        _windowService = windowService;
        _sessionId = sessionId;
    }
    
    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        return _windowService.OpenWindowInSession<TViewModel>(_sessionId);
    }
    
    public void CloseSession()
    {
        _windowService.CloseSession(_sessionId);
    }
}

// 3. Register shared service
builder.RegisterType<OrderBuilderService>()
       .As<IOrderBuilderService>()
       .InstancePerMatchingLifetimeScope("workflow-session");

// 4. Start workflow
var sessionId = _windowService.CreateSession("OrderWorkflow");
_windowService.OpenWindowInSession<WorkflowHostViewModel>(sessionId);

// 5. All windows in this session share the same IOrderBuilderService instance!
```

### Example 4: Shell ViewModel with Content Navigation

```csharp
public class WorkflowHostViewModel : ShellViewModel
{
    private readonly IOrderBuilderService _orderBuilder;
    
    public WorkflowHostViewModel(
        IContentManager contentManager,
        IWindowService windowService,
        IOrderBuilderService orderBuilder, // Shared across session!
        ILogger<WorkflowHostViewModel> logger) 
        : base(contentManager, windowService, logger)
    {
        _orderBuilder = orderBuilder;
    }
    
    public override async Task InitializeAsync()
    {
        Logger.LogInformation("Starting workflow - Order items: {Count}", 
            _orderBuilder.GetItems().Count);
        
        await ContentManager.NavigateToAsync<Step1ViewModel>();
    }
    
    [RelayCommand]
    private async Task NextStep()
    {
        await ContentManager.NavigateToAsync<Step2ViewModel>();
    }
}
```

---

## Advanced Scenarios

### 1. Shared Service in Workflow Session

**Registration:**

```csharp
// In Autofac module
builder.RegisterType<OrderBuilderService>()
       .As<IOrderBuilderService>()
       .InstancePerMatchingLifetimeScope("workflow-session");
```

**Usage:**

```csharp
// All windows opened in the same session share the SAME instance
var sessionId = _windowService.CreateSession("OrderWorkflow");

_windowService.OpenWindowInSession<OrderHostViewModel>(sessionId);
_windowService.OpenWindowInSession<ProductSelectorViewModel>(sessionId);
_windowService.OpenWindowInSession<CustomerSelectorViewModel>(sessionId);

// All 3 windows will receive the same IOrderBuilderService instance!
```

### 2. Parent-Child Window Relationships

```csharp
// Open parent window
var parentId = _windowService.OpenWindow<ParentViewModel>();

// Open child windows
var child1Id = _windowService.OpenChildWindow<ChildViewModel1>(parentId);
var child2Id = _windowService.OpenChildWindow<ChildViewModel2>(parentId);

// When parent closes, all children close automatically
_windowService.Close(parentId); // Also closes child1 and child2
```

### 3. Window Tracking by VmKey

```csharp
public class MyDetailViewModel : BaseViewModel
{
    private readonly IWindowService _windowService;
    
    [RelayCommand]
    private void CloseWindow()
    {
        // Close using VmKey (unique identifier for this ViewModel instance)
        _windowService.Close(this.GetVmKey());
    }
}
```

### 4. Shell Close Request from Content

```csharp
// In content ViewModel
public class FinalStepViewModel : BaseViewModel
{
    private readonly IContentManager _contentManager;
    
    [RelayCommand]
    private void Finish()
    {
        // Request shell to close
        _contentManager.RequestShellClose(
            showConfirmation: true, 
            "Save order before closing?");
    }
}

// Shell handles the request
protected override void OnShellCloseRequested(object? sender, ShellCloseRequestedEventArgs e)
{
    if (e.ShowConfirmation)
    {
        var result = MessageBox.Show(
            e.ConfirmationMessage ?? "Close?", 
            "Confirmation", 
            MessageBoxButton.YesNo);
            
        if (result == MessageBoxResult.Yes)
        {
            CloseShell();
        }
    }
    else
    {
        CloseShell();
    }
}
```

---

## Best Practices

### ✅ DO

1. **Always register ViewModels as `InstancePerDependency`**
   ```csharp
   builder.RegisterType<MyViewModel>().AsSelf().InstancePerDependency();
   ```

2. **Use `IVmParameters` for passing data to ViewModels**
   ```csharp
   public record CustomerDetailParams(int CustomerId) : ViewModelOptions, IVmParameters;
   ```

3. **Close windows using `VmKey` from ViewModels**
   ```csharp
   _windowService.Close(this.GetVmKey());
   ```

4. **Register shared services with exact string tags**
   ```csharp
   .InstancePerMatchingLifetimeScope("workflow-session")
   ```

5. **Use `ShellViewModel` for windows with content navigation**

6. **Implement `IDisposable` in ViewModels that need cleanup**

### ❌ DON'T

1. **Don't use `Guid.NewGuid()` as parent window ID**
   - Always track the actual window ID returned from `OpenWindow()`

2. **Don't register ViewModels as Singleton or PerLifetimeScope**
   - Each window needs its own ViewModel instance

3. **Don't use predicates in `InstancePerMatchingLifetimeScope`**
   ```csharp
   // ❌ BAD
   .InstancePerMatchingLifetimeScope((scope, reg) => scope.Tag.ToString().StartsWith("Session"))
   
   // ✅ GOOD
   .InstancePerMatchingLifetimeScope("workflow-session")
   ```

4. **Don't manually dispose window scopes**
   - Let `WindowService` handle scope disposal

5. **Don't keep strong references to windows**
   - `WindowService` uses `WeakReference<Window>` for automatic GC cleanup

---

## API Reference

### ScopeTag

```csharp
public readonly struct ScopeTag
{
    public ScopeCategory Category { get; }
    public string Name { get; }
    public Guid Id { get; }
    
    public static ScopeTag Root();
    public static ScopeTag Window(string windowName, Guid? id = null);
    public static ScopeTag WorkflowSession(string workflowName, Guid? id = null);
    public static ScopeTag Dialog(string dialogName, Guid? id = null);
    public static ScopeTag Custom(string customName, Guid? id = null);
}
```

### IWindowService

```csharp
public interface IWindowService
{
    // Session management
    Guid CreateSession(string sessionName);
    void CloseSession(Guid sessionId);
    
    // Open windows
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;
    Guid OpenWindow<TViewModel, TOptions>(TOptions options);
    Guid OpenWindowInSession<TViewModel>(Guid sessionId);
    Guid OpenWindowInSession<TViewModel, TOptions>(Guid sessionId, TOptions options);
    
    // Child windows
    Guid OpenChildWindow<TViewModel>(Guid parentWindowId);
    Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options);
    
    // Close windows
    void Close(Guid windowId);
    void Close(VmKey vmKey);
    void Close(Window window);
    void CloseWindow<TViewModel>(Guid windowId);
    void CloseAllChildWindows(Guid parentWindowId);
    
    // Events
    event EventHandler<WindowClosedEventArgs>? WindowClosed;
}
```

### IContentManager

```csharp
public interface IContentManager : INotifyPropertyChanged
{
    // Navigation
    Task NavigateToAsync<TViewModel>() where TViewModel : IViewModel;
    Task NavigateToAsync<TViewModel, TOptions>(TOptions options);
    Task NavigateBackAsync();
    
    // State
    object? CurrentContent { get; }
    bool CanNavigateBack { get; }
    int HistoryDepth { get; }
    
    // History
    void ClearHistory();
    
    // Shell control
    void RequestShellClose(bool showConfirmation = false, string? confirmationMessage = null);
    event EventHandler<ShellCloseRequestedEventArgs>? ShellCloseRequested;
}
```

### INavigationService

```csharp
public interface INavigationService : INotifyPropertyChanged
{
    // Navigation
    Task NavigateToAsync<TViewModel>() where TViewModel : IViewModel;
    Task NavigateToAsync<TViewModel, TOptions>(TOptions options);
    Task NavigateBackAsync();
    
    // State
    object? CurrentViewModel { get; }
    bool CanNavigateBack { get; }
    int HistoryDepth { get; }
    
    // History
    void ClearHistory();
    
    // Window control
    void RequestWindowClose(bool showConfirmation = false, string? confirmationMessage = null);
    event EventHandler<WindowCloseRequestedEventArgs>? WindowCloseRequested;
}
```

### IDialogService

```csharp
public interface IDialogService
{
    // Typed dialogs
    Task<TResult?> ShowDialogAsync<TViewModel, TResult>();
    Task<TResult?> ShowDialogAsync<TViewModel, TOptions, TResult>(TOptions options);
    
    // Common dialogs
    Task<MessageBoxResult> ShowMessageBoxAsync(string message, string? title = null, MessageBoxType type = MessageBoxType.Information);
    Task<bool> ShowConfirmationAsync(string message, string? title = null, string confirmText = "OK", string cancelText = "Cancel");
    Task ShowErrorAsync(string errorMessage, string? title = null);
    Task<string?> ShowInputAsync(string prompt, string? title = null, string? defaultValue = null);
}
```

---

## Testing

See [`WpfEngine.Tests/`](../WpfEngine.Tests/) for comprehensive unit tests including:

- ScopeTag definitions and extensions
- Content management and navigation
- View registry and locator
- ViewModel factory with parameter injection
- Window service session management
- Dialog service modal handling
- **STA thread tests** for WPF UI components using `[STAFact]`
- **Integration tests** for `InstancePerMatchingLifetimeScope` behavior

Run tests:
```bash
dotnet test
```

---

## License

[Your License Here]

---

## Contributing

[Contribution guidelines]

---

## Changelog

### v1.0.0
- Initial release
- Hierarchical scope system with ScopeTag
- WindowServiceRefactored with session support
- ContentManager for shell content navigation
- ShellViewModel base class
- Comprehensive test coverage (91 tests)

