# ✅ Refactoring Summary - WpfEngine Session Pattern

## 🎯 Co bylo provedeno

Vrátili jsme WpfEngine k původnímu přístupu z AutofacEnhancedWpfDemo a přidali **Workflow Session Pattern** pro sdílení services mezi okny.

---

## 📦 Změny v souborech

### **✅ WpfEngine (Knihovna)**

#### **Upraveno:**
1. `WpfEngine/Core/Views/Windows/ScopedWindow.cs`
   - Vráceno k původnímu přístupu
   - Okno vytváří vlastní child scope z parent scope
   - Lepší logování s ParentTag

2. `WpfEngine/Core/Services/IWindowService.cs`
   - Přidány session management metody
   - `CreateSession()`, `GetSessionScope()`, `CloseSession()`

3. `WpfEngine/Core/Services/Autofac/WindowService.cs`
   - Nový čistý soubor místo `window_service_autofac.cs`
   - Session support
   - Simplified scope handling

4. `WpfEngine/Configuration/CoreServicesModule.cs`
   - Registrace `IWorkflowSessionFactory`

#### **Nové soubory:**
1. `WpfEngine/Core/Services/IWorkflowSession.cs`
   - Interface pro workflow session
   - Interface pro WorkflowSessionFactory

2. `WpfEngine/Core/Services/Autofac/WorkflowSession.cs`
   - Implementace WorkflowSession
   - Implementace WorkflowSessionFactory

3. `WpfEngine/SESSION_PATTERN_GUIDE.md`
   - Kompletní dokumentace session pattern
   - Příklady použití

---

### **✅ WpfEngine.Demo (Demo aplikace)**

#### **Upraveno:**
1. `WpfEngine.Demo/ViewModels/DemoWorkflowHostViewModel.cs`
   - Používá `IWorkflowSessionFactory`
   - Vytváří session při inicializaci
   - Dispose session při ukončení

2. `WpfEngine.Demo/ViewModels/DemoWorkflowStep1ViewModel.cs`
   - Inject `IOrderBuilderService` (shared)
   - Ukládá customer do shared service

3. `WpfEngine.Demo/ViewModels/DemoWorkflowStep2ViewModel.cs`
   - Inject `IOrderBuilderService` (shared)
   - Přidává/odebírá items ze shared service
   - Real-time sync s shared službou

4. `WpfEngine.Demo/ViewModels/DemoWorkflowStep3ViewModel.cs`
   - Inject `IOrderBuilderService` (shared)
   - Čte items ze shared service
   - Čistí shared service po dokončení

5. `WpfEngine.Demo/Configuration/DemoModule.cs`
   - Registrace `WorkflowState` jako `InstancePerMatchingLifetimeScope`
   - Registrace `IOrderBuilderService` jako `InstancePerMatchingLifetimeScope`

6. `WpfEngine.Demo/ViewModels/BaseViewModel.cs`
   - Odebrán duplicate `Id` property

#### **Nové soubory:**
1. `WpfEngine.Demo/Views/ScopedWindow.cs`
   - Wrapper okolo WpfEngine.Core ScopedWindow
   - Pro Demo-specific customizace

2. `WpfEngine.Demo/Services/IOrderBuilderService.cs`
   - Shared service pro workflow
   - Demonstruje session-scoped sharing

#### **Smazáno:**
1. `WpfEngine.Demo/Views/ScopedWindowNew.cs` ❌ (obsolete)

---

## 🔧 Klíčové principy

### **1. Scope Hierarchie**

```
Root Application Scope
  │
  ├─ Window bez session (normal flow)
  │    └─ Window.Scope → ViewModels
  │
  └─ Workflow Session Scope ⭐ (NEW!)
       │
       ├─ Shared Services
       │    ├─ IOrderBuilderService (InstancePerMatchingLifetimeScope)
       │    └─ WorkflowState (InstancePerMatchingLifetimeScope)
       │
       ├─ Workflow Host Window.Scope
       │    └─ DemoWorkflowHostViewModel
       │         └─ Navigator → Step ViewModels (Step1, Step2, Step3)
       │
       ├─ Product Selector Window.Scope (budoucí rozšíření)
       │    └─ ProductSelectorViewModel
       │
       └─ Product Detail Window.Scope (budoucí rozšíření)
            └─ ProductDetailViewModel
```

### **2. Jak to funguje**

1. **DemoWorkflowHostViewModel vytvoří session:**
   ```csharp
   _session = _sessionFactory.CreateSession("order-creation-workflow");
   ```

2. **Session vytvoří scope s tagged názvem:**
   ```csharp
   var sessionScope = _rootScope.BeginLifetimeScope("workflow-session-{guid}");
   ```

3. **Window se vytvoří Z session scope jako parent:**
   ```csharp
   // Window dostane session scope jako parent
   // Window vytvoří child scope: session → window
   ```

4. **ViewModels resolvované V window scope dostanou shared services:**
   ```csharp
   // Autofac najde IOrderBuilderService v parent (session) scope
   // Všechny ViewModely v session vidí STEJNOU instanci!
   ```

---

## 📊 Registrace Pattern

| Lifetime | Kdy použít | Příklad |
|----------|------------|---------|
| **SingleInstance** | Celá aplikace | `IDbContextFactory` |
| **InstancePerLifetimeScope** | Per window/scope | `INavigator`, `IWindowManager` |
| **InstancePerMatchingLifetimeScope** | Per session | `IOrderBuilderService`, `WorkflowState` |
| **InstancePerDependency** | Vždy nový | ViewModels, Handlers |

---

## 🚀 Příklad použití

### **Základní workflow (bez extra oken):**

```csharp
// 1. Create session
var session = _sessionFactory.CreateSession("order-workflow");

// 2. Navigate through steps
await _navigator.NavigateToAsync<Step1>();  // Select customer
await _navigator.NavigateToAsync<Step2>();  // Add products  
await _navigator.NavigateToAsync<Step3>();  // Review & complete

// 3. All steps share IOrderBuilderService!

// 4. Close session
session.Dispose();
```

### **Advanced workflow (s extra okny):**

```csharp
// Ve Step2ViewModel:
[RelayCommand]
private void OpenProductSelector()
{
    // Otevře okno V SESSION - vidí shared service!
    _session.OpenWindow<ProductSelectorViewModel>();
}

// V ProductSelectorViewModel:
[RelayCommand]
private void OpenDetail(Product p)
{
    // Otevře child okno - také vidí shared service!
    _session.OpenChildWindow<ProductDetailViewModel>(
        parentWindowId: Id,
        new ProductDetailParams { ProductId = p.Id }
    );
}

// Všechna 3 okna (Step2, Selector, Detail) vidí STEJNÝ IOrderBuilderService!
```

---

## ✅ Výhody tohoto řešení

1. ✅ **Jasná odpovědnost:**
   - ScopedWindow vytváří vlastní scope
   - WindowService neřeší scope management
   - Session explicitně řídí shared services

2. ✅ **Flexibilní sdílení:**
   - `InstancePerMatchingLifetimeScope` umožňuje sdílet v session
   - `InstancePerLifetimeScope` pro per-window services
   - Kombinace obou patterns

3. ✅ **Real-time updates:**
   - Změny v shared service viditelné okamžitě
   - Žádné kopírování dat
   - Reference-based sharing

4. ✅ **Automatický cleanup:**
   - Dispose session → dispose scope → dispose shared services
   - Garbage collection friendly
   - Thread-safe

5. ✅ **Testovatelné:**
   - Můžeš mockovat `IWorkflowSession`
   - Můžeš mockovat `IOrderBuilderService`
   - Unit testy jsou jednoduché

---

## 🎓 Co se naučíš z tohoto refactoringu

### **Autofac Scope Hierarchie:**
- Jak vytvářet tagged scopes
- Jak `InstancePerMatchingLifetimeScope` funguje
- Parent-child scope relationships

### **DI Best Practices:**
- Separation of Concerns (Window vs WindowService vs Session)
- Factory Pattern (WorkflowSessionFactory)
- Service Locator Pattern (v rámci scope)

### **WPF Architecture:**
- ViewModel-First approach
- DataContext injection
- Window lifecycle management

---

## 📝 Migrace checklist

Pokud chceš migrovat existující kód na session pattern:

- [ ] Identifikuj, které services potřebuješ sdílet
- [ ] Zaregistruj je jako `InstancePerMatchingLifetimeScope("workflow-session-*")`
- [ ] Inject `IWorkflowSessionFactory` do host ViewModelu
- [ ] Vytvoř session v `InitializeAsync()`
- [ ] Inject shared services do step ViewModelů
- [ ] Dispose session v `Dispose()`
- [ ] Test!

---

## 🔮 Budoucí rozšíření

### **1. Multiple Sessions:**
```csharp
// Můžeš mít více sessions najednou
var session1 = _factory.CreateSession("order-workflow-1");
var session2 = _factory.CreateSession("order-workflow-2");

// Každá session má vlastní shared services!
```

### **2. Session Events:**
```csharp
_session.SessionClosed += (s, e) => {
    Logger.LogInformation("Session closed!");
};
```

### **3. Session Persistence:**
```csharp
// Budoucí enhancement - save/restore session state
await _session.SaveStateAsync();
await _session.RestoreStateAsync(sessionId);
```

---

**🎉 Hotovo! WpfEngine má teď čistou architekturu s podporou session pattern.**

