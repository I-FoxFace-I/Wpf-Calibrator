# WpfEngine Testing Guide

## 🎯 Philosophy

WpfEngine tests focus on **behavior verification** rather than implementation details. We test:
- ✅ Public API contracts
- ✅ State transitions
- ✅ Event notifications
- ✅ Lifecycle management
- ✅ Integration between components

We avoid:
- ❌ Testing private methods directly
- ❌ Over-mocking (test real integrations where possible)
- ❌ Testing framework internals (Autofac, WPF)

---

## 🏗️ Architecture Under Test

### **Core Components:**

```
┌─────────────────────────────────────────────────────────┐
│ ScopeTagDefinitions                                     │
│ ├─ ScopeCategory enum                                   │
│ └─ ScopeTag struct (type-safe scope identification)     │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ WindowServiceRefactored                                 │
│ ├─ CreateSession(name) → Guid                           │
│ ├─ OpenWindowInSession<TViewModel>(sessionId)           │
│ ├─ OpenChildWindow<TViewModel>(parentId)                │
│ └─ CloseSession(sessionId)                              │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ ContentManager                                          │
│ ├─ NavigateToAsync<TViewModel>()                        │
│ ├─ NavigateBackAsync()                                  │
│ ├─ CurrentContent property                              │
│ └─ ShellCloseRequested event                            │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ ViewRegistry                                            │
│ ├─ MapWindow<TViewModel, TWindow>()                     │
│ ├─ MapDialog<TViewModel, TDialog>()                     │
│ ├─ MapControl<TViewModel, TControl>()                   │
│ └─ TryGetViewType(vmType, out viewType)                 │
└─────────────────────────────────────────────────────────┘
```

---

## 🧪 Test Patterns

### **1. Scope Tag Testing**

**Purpose:** Verify type-safe scope identification

```csharp
[Fact]
public void ScopeTag_Window_CreatesCorrectTag()
{
    // Arrange
    var expectedGuid = Guid.NewGuid();

    // Act
    var tag = ScopeTag.Window("CustomerList", expectedGuid);

    // Assert
    tag.Category.Should().Be(ScopeCategory.Window);
    tag.Name.Should().Be("CustomerList");
    tag.Id.Should().Be(expectedGuid);
    tag.ToString().Should().Be($"Window:CustomerList:{expectedGuid}");
}
```

**What this tests:**
- Factory methods create correct category
- GUIDs are properly assigned
- String representation is correct
- Tag equality works

---

### **2. Content Manager Testing**

**Purpose:** Verify shell content navigation behavior

```csharp
[Fact]
public async Task NavigateToAsync_WithParameters_PassesOptionsCorrectly()
{
    // Arrange
    var contentManager = CreateContentManager();
    var options = new TestParams { Value = "test" };

    // Act
    await contentManager.NavigateToAsync<TestViewModel, TestParams>(options);

    // Assert
    var vm = contentManager.CurrentContent as TestViewModel;
    vm!.ReceivedOptions.Should().Be(options);
    vm.ReceivedOptions.Value.Should().Be("test");
}
```

**What this tests:**
- Parameters are passed through
- ViewModel is initialized with options
- Content is updated correctly

---

### **3. Session Management Testing**

**Purpose:** Verify session lifecycle and scope isolation

```csharp
[Fact]
public void CreateSession_CreatesIsolatedScope()
{
    // Arrange
    var windowService = CreateWindowService();

    // Act
    var session1 = windowService.CreateSession("session-1");
    var session2 = windowService.CreateSession("session-2");

    // Assert
    session1.Should().NotBe(session2);
    // Each session creates independent scope
    // Services registered as InstancePerMatchingLifetimeScope
    // will have separate instances per session
}
```

**What this tests:**
- Sessions are independent
- Session IDs are unique
- Sessions can coexist

---

### **4. View Mapping Testing**

**Purpose:** Verify ViewModel-to-View configuration

```csharp
[Fact]
public void MapWindow_OverwritesExistingMapping()
{
    // Arrange
    var registry = new ViewRegistry(Mock.Of<ILogger<ViewRegistry>>());
    registry.MapWindow<TestViewModel, TestWindow1>();

    // Act
    registry.MapWindow<TestViewModel, TestWindow2>();

    // Assert
    registry.TryGetViewType(typeof(TestViewModel), out var viewType)
        .Should().BeTrue();
    viewType.Should().Be(typeof(TestWindow2));
}
```

**What this tests:**
- Mappings can be updated
- Latest mapping wins
- Query returns correct type

---

## 🔧 Test Utilities

### **AutofacTestFixture**

Base class for tests requiring Autofac container:

```csharp
public class MyTests : AutofacTestFixture
{
    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register your test ViewModels and Views
        builder.RegisterType<MyTestViewModel>().AsSelf();
        builder.RegisterType<MyTestWindow>().AsSelf();
    }

    [Fact]
    public void MyTest()
    {
        // Arrange
        var vm = Resolve<MyTestViewModel>();

        // Act & Assert
        // ...
    }
}
```

**Benefits:**
- ✅ Automatic container setup
- ✅ Proper disposal
- ✅ Core services pre-registered
- ✅ Easy customization

---

## 📊 Testing Scenarios

### **Scenario 1: Simple Navigation**

```csharp
// Given: Shell window with ContentManager
var contentManager = CreateContentManager();

// When: Navigate to Step1
await contentManager.NavigateToAsync<Step1ViewModel>();

// Then: CurrentContent is Step1ViewModel
contentManager.CurrentContent.Should().BeOfType<Step1ViewModel>();

// When: Navigate to Step2
await contentManager.NavigateToAsync<Step2ViewModel>();

// Then: Can navigate back
contentManager.CanNavigateBack.Should().BeTrue();
contentManager.HistoryDepth.Should().Be(1);

// When: Navigate back
await contentManager.NavigateBackAsync();

// Then: CurrentContent is Step1ViewModel again
contentManager.CurrentContent.Should().BeOfType<Step1ViewModel>();
```

---

### **Scenario 2: Session with Shared Services**

```csharp
// Given: WindowService and session-scoped service
var windowService = CreateWindowService();
var sessionId = windowService.CreateSession("workflow");

// Register shared service
builder.RegisterType<OrderBuilder>()
       .As<IOrderBuilder>()
       .InstancePerMatchingLifetimeScope((scope, _) =>
           scope.Tag?.ToString()?.StartsWith("WorkflowSession:") ?? false);

// When: Open two windows in session
var window1Id = windowService.OpenWindowInSession<Window1ViewModel>(sessionId);
var window2Id = windowService.OpenWindowInSession<Window2ViewModel>(sessionId);

// Then: Both windows see SAME IOrderBuilder instance
var builder1 = GetServiceFromWindow<IOrderBuilder>(window1Id);
var builder2 = GetServiceFromWindow<IOrderBuilder>(window2Id);
builder1.Should().BeSameAs(builder2);

// When: Close session
windowService.CloseSession(sessionId);

// Then: All windows closed, service disposed
```

---

### **Scenario 3: View Mapping Configuration**

```csharp
// Given: ViewRegistry
var registry = new ViewRegistry(logger);

// When: Configure mappings
registry.MapWindow<CustomerListViewModel, CustomerListWindow>()
        .MapWindow<ProductListViewModel, ProductListWindow>()
        .MapDialog<EditCustomerViewModel, EditCustomerDialog>()
        .MapControl<WorkflowStepViewModel, WorkflowStepView>();

// Then: Can resolve all mappings
registry.TryGetViewType(typeof(CustomerListViewModel), out var view1);
view1.Should().Be(typeof(CustomerListWindow));

registry.TryGetViewType(typeof(EditCustomerViewModel), out var view2);
view2.Should().Be(typeof(EditCustomerDialog));
```

---

## 🎓 Best Practices

### **1. Test One Thing**

```csharp
// ❌ Bad - Tests multiple behaviors
[Fact]
public void NavigateAsync_DoesEverything()
{
    await manager.NavigateToAsync<VM1>();
    // Assert navigation
    await manager.NavigateBackAsync();
    // Assert back navigation
    manager.ClearHistory();
    // Assert clearing
}

// ✅ Good - One behavior per test
[Fact]
public void NavigateAsync_SetsCurrentContent() { }

[Fact]
public void NavigateBackAsync_RestoresPrevious() { }

[Fact]
public void ClearHistory_DisposesAllItems() { }
```

### **2. Use Descriptive Names**

```csharp
// ❌ Bad
[Fact]
public void Test1() { }

// ✅ Good
[Fact]
public void NavigateToAsync_WithNullHistory_DoesNotThrow() { }

[Fact]
public void CreateSession_WithDuplicateName_CreatesUniqueSessions() { }
```

### **3. Arrange-Act-Assert Clearly**

```csharp
[Fact]
public void Example_Test()
{
    // Arrange - Setup
    var manager = CreateManager();
    var options = new TestOptions();

    // Act - Execute
    await manager.DoSomethingAsync(options);

    // Assert - Verify
    manager.State.Should().Be(ExpectedState);
}
```

### **4. Test Disposal**

```csharp
[Fact]
public void NavigateAsync_DisposesPreviousContent()
{
    // Arrange
    await manager.NavigateToAsync<DisposableViewModel>();
    var firstVm = manager.CurrentContent as DisposableViewModel;

    // Act
    await manager.NavigateToAsync<AnotherViewModel>();

    // Assert
    firstVm!.IsDisposed.Should().BeTrue();
}
```

---

## 🚦 Running Tests

### **Run all tests:**
```bash
cd WpfEngine.Tests
dotnet test
```

### **Run with detailed output:**
```bash
dotnet test --logger "console;verbosity=detailed"
```

### **Run specific test class:**
```bash
dotnet test --filter "FullyQualifiedName~ScopeTagDefinitionsTests"
```

### **Run tests matching pattern:**
```bash
dotnet test --filter "Name~Navigation"
```

### **Generate coverage report:**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 📈 Coverage Goals

### **Current Coverage:**
- ✅ ScopeTagDefinitions - 100%
- ✅ ViewRegistry - ~90%
- ✅ ContentManager - ~85%
- ⬜ WindowServiceRefactored - ~60% (integration tests)
- ⬜ ShellViewModel - ~70%

### **Priority for Expansion:**
1. WindowService full lifecycle tests
2. Session service sharing verification
3. Concurrent operation tests
4. Error handling and edge cases
5. Performance benchmarks

---

## 🐛 Debugging Tests

### **Tips:**

1. **Set breakpoints** in test methods
2. **Use `Should().BeTrue()` instead of `Assert.True()`** for better error messages
3. **Run single test** to isolate issues
4. **Check test output** for logged messages
5. **Use `[Fact(Skip = "Reason")]`** to temporarily disable flaky tests

### **Common Issues:**

**Problem:** Test hangs
```csharp
// Solution: Add timeout
[Fact(Timeout = 5000)]
public async Task MyTest() { }
```

**Problem:** Disposal errors
```csharp
// Solution: Ensure proper cleanup
public void Dispose()
{
    _scope?.Dispose();  // Dispose in correct order
    _container?.Dispose();
}
```

**Problem:** Autofac resolution errors
```csharp
// Solution: Check registrations
protected override void RegisterTestServices(ContainerBuilder builder)
{
    builder.RegisterType<MyTestType>().AsSelf();
}
```

---

## 📚 Resources

### **Frameworks Used:**
- **xUnit** - Test framework
- **FluentAssertions** - Readable assertions
- **Moq** - Mocking framework
- **Autofac.Extras.Moq** - Auto-mocking with Autofac

### **Documentation:**
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq Quickstart](https://github.com/moq/moq4)
- [Autofac Testing](https://autofac.readthedocs.io/en/latest/advanced/testing.html)

---

## ✅ Checklist for New Tests

When adding new test class:
- [ ] Inherit from `AutofacTestFixture` if using DI
- [ ] Implement `IDisposable` for cleanup
- [ ] Follow AAA pattern
- [ ] Use FluentAssertions
- [ ] Name tests descriptively
- [ ] Add XML comments to test class
- [ ] Group related tests
- [ ] Test both success and failure cases
- [ ] Verify disposal/cleanup behavior

---

**Happy Testing! May your tests be green and your bugs be few! 🧪✨**

