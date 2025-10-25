# WpfEngine.Tests

Unit and integration tests for WpfEngine library.

## 🎯 Overview

This test project provides comprehensive coverage for WpfEngine's core functionality, focusing on:
- **Scope management** - Type-safe scope tags and hierarchies
- **Content navigation** - Shell content management
- **View mapping** - ViewModel-to-View registration
- **Window lifecycle** - Window service with session support

## 📁 Test Structure

```
WpfEngine.Tests/
├── Core/
│   ├── Scopes/
│   │   └── ScopeTagDefinitionsTests.cs     - Type-safe scope tag tests
│   ├── Services/
│   │   ├── ContentManagerTests.cs          - Shell content navigation tests
│   │   ├── ViewRegistryTests.cs            - View mapping tests
│   │   └── Autofac/
│   │       └── WindowServiceRefactoredTests.cs - Window service tests
│   └── ViewModels/
│       └── ShellViewModelTests.cs          - Shell base class tests
├── Helpers/
│   └── AutofacTestFixture.cs               - Autofac test helpers
└── README.md                                - This file
```

## 🧪 Test Categories

### **1. Scope Tag Tests** (`ScopeTagDefinitionsTests`)

Tests for the type-safe scope tagging system:

```csharp
// Creating different scope categories
var windowTag = ScopeTag.Window("CustomerList", guid);
var sessionTag = ScopeTag.WorkflowSession("order-workflow", guid);
var dialogTag = ScopeTag.Dialog("ConfirmDialog", guid);

// Tag comparison and equality
tag1.Should().Be(tag2);  // Same category, name, and ID
tag1.IsWindow().Should().BeTrue();
tag2.IsWorkflowSession().Should().BeTrue();

// String representations
tag.ToString();       // "Window:CustomerList:{guid}"
tag.ToShortString();  // "Window:CustomerList"
```

**Key Tests:**
- ✅ Factory methods create correct categories
- ✅ GUIDs are generated or accepted
- ✅ Equality comparison works correctly
- ✅ Extension methods identify categories
- ✅ String formatting is correct

---

### **2. Content Manager Tests** (`ContentManagerTests`)

Tests for shell content navigation within a window:

```csharp
// Navigation within shell
await contentManager.NavigateToAsync<Step1ViewModel>();
contentManager.CurrentContent.Should().BeOfType<Step1ViewModel>();

// Navigation with parameters
var options = new StepParams { CustomerId = 123 };
await contentManager.NavigateToAsync<Step2ViewModel, StepParams>(options);

// History management
contentManager.CanNavigateBack.Should().BeTrue();
await contentManager.NavigateBackAsync();
contentManager.HistoryDepth.Should().Be(0);

// Shell close request
contentManager.RequestShellClose(showConfirmation: true);
// Event is raised for ShellViewModel to handle
```

**Key Tests:**
- ✅ Navigation sets current content
- ✅ ViewModels are initialized
- ✅ Previous content is pushed to history
- ✅ Previous content is disposed on navigation
- ✅ Parameters are passed correctly
- ✅ Back navigation restores previous content
- ✅ History can be cleared
- ✅ Shell close events are raised
- ✅ PropertyChanged is raised on content change

---

### **3. View Registry Tests** (`ViewRegistryTests`)

Tests for ViewModel-to-View mapping configuration:

```csharp
// Register mappings
registry.MapWindow<ProductsViewModel, ProductsWindow>();
registry.MapDialog<EditProductViewModel, EditProductDialog>();
registry.MapControl<WorkflowStepViewModel, WorkflowStepView>();

// Query mappings
registry.TryGetViewType(typeof(ProductsViewModel), out var viewType);
viewType.Should().Be(typeof(ProductsWindow));

// Mapping management
registry.RemoveMapping<ProductsViewModel>();
registry.Clear();  // Removes all
```

**Key Tests:**
- ✅ Window mappings are registered
- ✅ Dialog mappings are registered
- ✅ Control mappings are registered
- ✅ Mappings can be overwritten
- ✅ Mappings can be removed
- ✅ All mappings can be cleared
- ✅ All mappings can be queried

---

### **4. Window Service Tests** (`WindowServiceRefactoredTests`)

Integration tests for window management with session support:

```csharp
// Session management
var sessionId = windowService.CreateSession("workflow-session");
sessionId.Should().NotBe(Guid.Empty);

// Open window in session
var windowId = windowService.OpenWindowInSession<ProductSelectorViewModel>(sessionId);

// Multiple sessions
var session1 = windowService.CreateSession("session-1");
var session2 = windowService.CreateSession("session-2");
// Each session has independent scope

// Close session and all windows
windowService.CloseSession(sessionId);
// All windows in session are closed
// Session scope is disposed
```

**Key Tests:**
- ✅ Sessions can be created
- ✅ Each session has unique ID
- ✅ Sessions can be closed
- ✅ Opening window in non-existent session throws
- ✅ Windows in session share session-scoped services (integration test)

---

## 🔧 Running Tests

### **Run all tests:**
```bash
cd WpfEngine.Tests
dotnet test
```

### **Run with coverage:**
```bash
dotnet test /p:CollectCoverage=true
```

### **Run specific test:**
```bash
dotnet test --filter "FullyQualifiedName~ScopeTagDefinitionsTests"
```

### **Run tests by category:**
```bash
# Scope tests
dotnet test --filter "FullyQualifiedName~Scopes"

# Service tests
dotnet test --filter "FullyQualifiedName~Services"
```

---

## 📝 Testing Patterns

### **1. Arrange-Act-Assert (AAA)**

All tests follow the AAA pattern:

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies
    var tag = ScopeTag.Window("Test");

    // Act - Execute the method under test
    var result = tag.ToString();

    // Assert - Verify the outcome
    result.Should().Be("Window:Test:{guid}");
}
```

### **2. Test Fixtures**

For tests requiring Autofac container:

```csharp
public class MyTests : IDisposable
{
    private readonly IContainer _container;

    public MyTests()
    {
        var builder = new ContainerBuilder();
        // Register dependencies
        _container = builder.Build();
    }

    public void Dispose()
    {
        _container?.Dispose();
    }
}
```

### **3. FluentAssertions**

Using FluentAssertions for readable assertions:

```csharp
// Instead of:
Assert.Equal(expected, actual);
Assert.True(condition);

// Use:
actual.Should().Be(expected);
condition.Should().BeTrue();
result.Should().NotBeNull();
collection.Should().HaveCount(5);
```

---

## 🎓 Key Concepts Tested

### **Hierarchical Scope Pattern**

```
Root Scope
  └─ Session Scope ("WorkflowSession:order:{guid}")
       ├─ Shared Services (InstancePerMatchingLifetimeScope)
       │
       ├─ Window1.Scope ("Window:Host:{guid}")
       │    ├─ Window services (InstancePerMatchingLifetimeScope("Window:*"))
       │    └─ Content ViewModels (from window scope, see session services!)
       │
       └─ Window2.Scope ("Window:Selector:{guid}")
            └─ ViewModel (from session scope, sees SAME shared services!)
```

**What we test:**
- ✅ Scope tags are created correctly
- ✅ Content is resolved from correct scope
- ✅ Services are shared via InstancePerMatchingLifetimeScope
- ✅ Session lifecycle (create, use, dispose)

### **Content Navigation Pattern**

```
ShellViewModel
  ├─ IContentManager (from window scope)
  │    ├─ NavigateToAsync<Step1>()
  │    ├─ NavigateToAsync<Step2>()
  │    └─ NavigateBackAsync()
  │
  └─ CurrentContent (bind to ContentControl)
       └─ Step ViewModels (from window scope)
```

**What we test:**
- ✅ Navigation updates CurrentContent
- ✅ History is maintained
- ✅ ViewModels are initialized
- ✅ Previous content is disposed
- ✅ PropertyChanged events fire

### **View Mapping Pattern**

```csharp
// Configuration
registry.MapWindow<ProductsViewModel, ProductsWindow>();
registry.MapDialog<EditProductViewModel, EditProductDialog>();
registry.MapControl<StepViewModel, StepView>();

// Runtime usage
var view = viewLocator.ResolveView<ProductsViewModel>();
// Returns ProductsWindow instance
```

**What we test:**
- ✅ Mappings are registered correctly
- ✅ Mappings can be queried
- ✅ Mappings can be overwritten/removed
- ✅ Different view types (Window, Dialog, Control)

---

## 📚 Additional Resources

### **Testing WPF Applications:**
- WPF applications require STAThread for UI tests
- Most tests focus on ViewModels (no UI required)
- Integration tests may need UI thread dispatcher

### **Autofac Testing:**
- Use `Autofac.Extras.Moq` for auto-mocking
- Create test containers in test fixtures
- Dispose containers in test cleanup

### **FluentAssertions:**
- More readable assertions
- Better error messages
- Chainable syntax

---

## 🔍 Test Coverage

Current coverage focuses on:
- ✅ Core abstractions (scopes, services)
- ✅ Autofac-specific implementations
- ✅ Navigation and lifecycle management
- ⬜ UI integration tests (future)
- ⬜ End-to-end workflow tests (future)

---

## 🚀 Future Enhancements

Potential test additions:
- [ ] WindowService full integration tests with real windows
- [ ] Session service sharing verification tests
- [ ] ViewModelFactory parameter injection tests
- [ ] ShellViewModel lifecycle tests
- [ ] Disposal chain verification tests
- [ ] Performance tests for large hierarchies
- [ ] Thread-safety tests for concurrent operations

---

## 💡 Contributing

When adding new tests:
1. Follow AAA pattern
2. Use FluentAssertions
3. Name tests descriptively: `MethodName_Scenario_ExpectedBehavior`
4. Add XML comments for test classes
5. Dispose resources in test cleanup
6. Group related tests in nested classes if needed

---

**Happy Testing! 🧪**

