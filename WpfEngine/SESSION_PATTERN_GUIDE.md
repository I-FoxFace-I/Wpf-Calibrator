# 🚀 Workflow Session Pattern - Průvodce

## Co je Workflow Session Pattern?

**Workflow Session Pattern** umožňuje sdílet services napříč více okny v rámci jedné "session" (relace).

### Hierarchie Scope:

```
Root Application Scope
  │
  └─ Workflow Session Scope (tag: "workflow-session-{guid}")
       │
       ├─ Shared Services (InstancePerMatchingLifetimeScope)
       │    ├─ IOrderBuilderService ← JEDNA instance pro celou session
       │    └─ WorkflowState ← JEDNA instance pro celou session
       │
       ├─ Window1.Scope (Workflow Host)
       │    └─ DemoWorkflowHostViewModel
       │         └─ Navigator → Step ViewModels
       │
       ├─ Window2.Scope (Product Selector - opened from session)
       │    └─ ProductSelectorViewModel ← Vidí STEJNÝ IOrderBuilderService!
       │
       └─ Window3.Scope (Product Detail - child of Window2)
            └─ ProductDetailViewModel ← Vidí STEJNÝ IOrderBuilderService!
```

---

## 📝 Jak používat Session Pattern

### **1. Registrace Shared Services**

```csharp
// DemoModule.cs
public class DemoModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Service sdílený v rámci workflow session
        builder.RegisterType<OrderBuilderService>()
               .As<IOrderBuilderService>()
               .InstancePerMatchingLifetimeScope("workflow-session-*");
        
        // Workflow state také shared
        builder.RegisterType<WorkflowState>()
               .AsSelf()
               .InstancePerMatchingLifetimeScope("workflow-session-*");
    }
}
```

**Klíč:** `InstancePerMatchingLifetimeScope("workflow-session-*")`
- Autofac vytvoří JEDNU instanci pro každý scope, jehož tag začíná na "workflow-session-"
- Všechna okna vytvořená v rámci session budou sdílet tuto instanci

---

### **2. Vytvoření Session v Host ViewModelu**

```csharp
public partial class DemoWorkflowHostViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IWorkflowSessionFactory _sessionFactory;
    private IWorkflowSession? _session;

    public DemoWorkflowHostViewModel(
        INavigationService navigator,
        IWorkflowSessionFactory sessionFactory,  // Inject factory
        ILogger<DemoWorkflowHostViewModel> logger) : base(logger)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task InitializeAsync()
    {
        // Vytvoř workflow session
        _session = _sessionFactory.CreateSession("order-creation-workflow");
        
        Logger.LogInformation("Workflow session created: {SessionId}", _session.SessionId);
        
        // Naviguj na první krok
        await _navigator.NavigateToAsync<DemoWorkflowStep1ViewModel>();
    }

    public void Dispose()
    {
        // Zavři session (zavře všechna okna v session)
        _session?.Dispose();
    }
}
```

---

### **3. Použití Shared Service ve ViewModelu**

```csharp
public partial class DemoWorkflowStep2ViewModel : BaseViewModel
{
    private readonly IOrderBuilderService _orderBuilder;  // Shared!

    public DemoWorkflowStep2ViewModel(
        IOrderBuilderService orderBuilder,  // Autofac injectuje STEJNOU instanci
        ...)                                 // pro všechny ViewModely v session!
    {
        _orderBuilder = orderBuilder;
    }

    [RelayCommand]
    private void AddProduct(DemoProduct product)
    {
        // Přidej do shared service
        _orderBuilder.AddItem(product.Id, product.Name, product.Price, Quantity);
        
        // Všechna ostatní okna v session vidí okamžitě změnu!
    }
}
```

---

### **4. Otevření Okna v Session**

#### **A) Z Host ViewModelu:**

```csharp
public partial class DemoWorkflowHostViewModel
{
    [RelayCommand]
    private void OpenProductSelector()
    {
        // Otevře okno v rámci session
        _session?.OpenWindow<ProductSelectorViewModel>();
    }
    
    [RelayCommand]
    private void OpenProductSelectorWithParams()
    {
        // S parametry
        _session?.OpenWindow<ProductSelectorViewModel, ProductSelectorParams>(
            new ProductSelectorParams { FilterCategory = "Electronics" }
        );
    }
}
```

#### **B) Z jiného okna v Session (child window):**

```csharp
public partial class ProductSelectorViewModel
{
    private readonly IWorkflowSession _session;  // Inject session!
    
    [RelayCommand]
    private void OpenProductDetail(DemoProduct product)
    {
        // Otevře child okno v session
        _session.OpenChildWindow<ProductDetailViewModel, ProductDetailParams>(
            parentWindowId: Id,  // This ViewModel's window ID
            new ProductDetailParams { ProductId = product.Id }
        );
    }
}
```

**DŮLEŽITÉ:** Pro child windows musíš mít přístup k `IWorkflowSession`!

---

## 🎯 Scénář: Product Selector s Detail View

### **Požadavek:**
- Workflow má seznam produktů v samostatném okně
- Z tohoto okna lze otevřít detail produktu
- Obě okna musí vidět stejný `IOrderBuilderService`
- Výběr produktu se přidá do shared service

### **Řešení:**

#### **1. Product Selector ViewModel:**

```csharp
public partial class ProductSelectorViewModel : BaseViewModel, IInitializable
{
    private readonly IOrderBuilderService _orderBuilder;  // Shared!
    private readonly IWorkflowSession _session;           // Pro otevírání child oken
    
    public ProductSelectorViewModel(
        IOrderBuilderService orderBuilder,
        IWorkflowSession session,  // Inject session
        ...)
    {
        _orderBuilder = orderBuilder;
        _session = session;
    }
    
    [RelayCommand]
    private void ViewProductDetail(DemoProduct product)
    {
        // Otevře detail v rámci session
        _session.OpenChildWindow<ProductDetailViewModel, ProductDetailParams>(
            Id,  // Parent window ID
            new ProductDetailParams { ProductId = product.Id }
        );
    }
    
    [RelayCommand]
    private void SelectProduct(DemoProduct product)
    {
        // Přidá do shared service
        _orderBuilder.AddItem(product.Id, product.Name, product.Price, 1);
        
        Logger.LogInformation("Product {ProductName} added to order", product.Name);
        
        // Zavře okno - změny zůstanou v shared service!
        // TODO: Close via WindowService
    }
}
```

#### **2. Product Detail ViewModel:**

```csharp
public partial class ProductDetailViewModel : BaseViewModel
{
    private readonly IOrderBuilderService _orderBuilder;  // STEJNÁ instance!
    
    public ProductDetailViewModel(
        IOrderBuilderService orderBuilder,
        ...)
    {
        _orderBuilder = orderBuilder;
    }
    
    [RelayCommand]
    private void AddToOrder()
    {
        // Přidá do STEJNÉHO shared service jako ProductSelector!
        _orderBuilder.AddItem(ProductId, ProductName, Price, Quantity);
        
        Logger.LogInformation("Product added from detail view");
        
        // Zavři okno
    }
}
```

#### **3. Otevření Product Selector z Workflow:**

```csharp
// Ve DemoWorkflowHostViewModel nebo Step2ViewModel:

[RelayCommand]
private void OpenProductSelector()
{
    // Otevře v session
    _session?.OpenWindow<ProductSelectorViewModel>();
}
```

---

## ✅ Výhody Session Pattern

1. ✅ **Explicitní sdílení** - vidíš, že service je shared
2. ✅ **Scope izolace** - každá session má vlastní shared services
3. ✅ **Automatický cleanup** - dispose session → dispose všech shared services
4. ✅ **Flexibilní hierarchie** - můžeš otevírat okna a child okna
5. ✅ **Testovatelné** - můžeš mockovat shared services
6. ✅ **Thread-safe** - každá session je izolovaná

---

## 📊 Comparison: State vs Shared Service

| Přístup | Předávání dat | Real-time updates | Komplexita |
|---------|---------------|-------------------|------------|
| **WorkflowState** (original) | Kopírování mezi kroky | ❌ Ne | Střední |
| **Shared Service** (new) | Reference na shared objekt | ✅ Ano | Nízká |

---

## 🎨 Registrace Pattern

```csharp
// Singleton - celá aplikace
builder.RegisterType<IDbContextFactory>()
       .SingleInstance();

// Per Window - každé okno má vlastní
builder.RegisterType<INavigator>()
       .InstancePerLifetimeScope();

// Per Session - sdílený v rámci workflow session
builder.RegisterType<IOrderBuilderService>()
       .InstancePerMatchingLifetimeScope("workflow-session-*");

// Transient - nový při každém resolve
builder.RegisterType<ProductViewModel>()
       .InstancePerDependency();
```

---

## 🔧 Debugging Tips

### **Logování:**

```
[SESSION_FACTORY] Created workflow session {SessionId} with tag 'order-creation-workflow'
[SCOPED_WINDOW] DemoWorkflowHostWindow created with scope (Tag: demo-workflow-host, ParentTag: workflow-session-...)
[WORKFLOW] Step2 ViewModel created with shared OrderBuilder service
```

Sleduj "ParentTag" v logách - měl by ukazovat "workflow-session-*" pro okna v session.

### **Ověření sdílení:**

```csharp
// V konstruktoru ViewModelu:
Logger.LogInformation("OrderBuilder instance ID: {InstanceId}", 
    _orderBuilder.GetHashCode());

// Ve všech ViewModelech v session by měl být STEJNÝ hash!
```

---

## 📚 Kdy použít Session Pattern?

✅ **ANO:**
- Multi-window workflows (vytváření objednávky)
- Sdílený draft state mezi okny
- Real-time collaboration mezi views
- Shopping cart sdílený mezi catalog a detail

❌ **NE:**
- Jednoduchá okna bez souvislosti
- Data persistence (použij database)
- Global state (použij Singleton)
- Communication mezi nesouvisejícími okny (použij events)

---

## 💡 Advanced: Custom Session Tags

```csharp
// Pro různé typy workflows:

// Order creation workflow
_sessionFactory.CreateSession("order-workflow");

// Customer onboarding workflow
_sessionFactory.CreateSession("onboarding-workflow");

// Pak registruj services specificky:
builder.RegisterType<OrderBuilderService>()
       .InstancePerMatchingLifetimeScope("order-workflow");

builder.RegisterType<OnboardingService>()
       .InstancePerMatchingLifetimeScope("onboarding-workflow");
```

---

**Hotovo! 🎉 Workflow Session Pattern je plně implementován.**

