# Autofac WPF Demo - Refactored

## 🎯 Hlavní změny

### 1. **ViewModel-First Architecture**
- ViewModels jsou resolvovány první
- ViewLocator najde odpovídající Window podle konvence (`ProductsViewModel` → `ProductsWindow`)
- DataContext nastaven automaticky pomocí Navigator service

### 2. **Navigator Service**
- Centralizovaná navigace mezi okny
- Modal dialogy s typovanými výsledky: `ShowDialogAsync<TViewModel, TResult>()`
- Non-modal okna: `ShowWindow<TViewModel>()`
- Podpora pro přepínání ViewModelu v rámci okna: `NavigateTo<TViewModel>()`

### 3. **IDbContextFactory Pattern**
- Singleton factory místo scoped DbContext
- Každá operace vytváří vlastní DbContext instanci
- Lepší izolace, thread-safety a testovatelnost
- SQLite databáze místo InMemory

### 4. **CQRS Pattern**
- Commands (write operations) a Queries (read operations)
- Handlers pro každou operaci
- Čistá separace business logiky od ViewModels
- Snadné testování a rozšiřování

### 5. **CommunityToolkit.Mvvm**
- Source generators pro INotifyPropertyChanged
- `[ObservableProperty]` atribut místo manuálních property
- `[RelayCommand]` atribut pro commands
- Méně boilerplate kódu

### 6. **Modern UI Design**
- Inspirace Material Design / Fluent UI
- Shadows, rounded corners, modern color palette
- Responzivní layout
- Loading states a error handling v UI

### 7. **ScopedWindow Redesign**
- **Není IDisposable** - scope je disposována interně
- ViewModel jako DataContext (nastaveno z venku)
- Automatické dispose scope při zavření okna
- Jednodušší a čistší implementace

### 8. **Autofac + MS.DI Integration**
- `builder.Populate(services)` pro MS.DI služby
- Logging, Configuration z MS.Extensions
- Autofac features (modules, scopes, parametrizace)
- Best of both worlds

## 📁 Struktura projektu

```
AutofacEnhancedWpfDemo/
├── Application/              # CQRS
│   ├── ICommand.cs
│   ├── IQuery.cs
│   ├── Products/
│   │   ├── Commands
│   │   ├── Queries
│   │   └── Handlers
│   └── ...
├── Configuration/            # Autofac Modules
│   ├── DataModule.cs        (IDbContextFactory)
│   ├── ServicesModule.cs    (Navigator, Handlers)
│   └── ViewsModule.cs       (Views, ViewModels)
├── Converters/              # Value Converters
│   └── ValueConverters.cs
├── Data/
│   ├── AppDbContext.cs      (SQLite)
│   └── DbSeeder.cs
├── Models/
│   ├── Customer.cs
│   ├── Product.cs
│   ├── Order.cs
│   └── OrderItem.cs
├── Services/
│   ├── INavigator.cs        # Nový!
│   ├── Navigator.cs
│   ├── IViewLocator.cs      # Nový!
│   └── ViewLocator.cs
├── ViewModels/
│   ├── BaseViewModel.cs     (CommunityToolkit.Mvvm)
│   ├── MainMenuViewModel.cs
│   ├── ProductsViewModel.cs
│   ├── EditProductViewModel.cs
│   └── ...
├── Views/
│   ├── ScopedWindow.cs      (Updated)
│   ├── MainMenuWindow.xaml
│   ├── ProductsWindow.xaml  (Modern design)
│   ├── EditProductWindow.xaml
│   └── ...
├── App.xaml
├── App.xaml.cs              (Bootstrap with Autofac + MS.DI)
└── AutofacEnhancedWpfDemo.csproj
```

## 🚀 Jak to funguje

### Otevření okna
```csharp
// V MainMenuViewModel
_navigator.ShowWindow<ProductsViewModel>();

// Navigator:
// 1. Resolvuje ProductsViewModel
// 2. ViewLocator najde ProductsWindow
// 3. Nastaví VM jako DataContext
// 4. window.Show()
```

### Modal dialog s výsledkem
```csharp
// V ProductsViewModel
var result = await _navigator.ShowDialogAsync<EditProductViewModel, EditProductResult>(
    new EditProductParams { ProductId = 123 }
);

if (result?.Success == true)
{
    await LoadProductsAsync();
}

// V EditProductViewModel
await _updateHandler.HandleAsync(new UpdateProductCommand(...));
_navigator.CloseDialog<EditProductViewModel>(new EditProductResult { Success = true });
```

### CQRS operace
```csharp
// Query
var products = await _getAllProductsHandler.HandleAsync(new GetAllProductsQuery());

// Command
await _updateProductHandler.HandleAsync(new UpdateProductCommand(id, name, price, stock));
```

### DbContext usage
```csharp
// V Handleru
await using var context = await _contextFactory.CreateDbContextAsync();
var product = await context.Products.FindAsync(id);
product.Price = newPrice;
await context.SaveChangesAsync();
// context se automaticky dispose
```

## 🎨 UI Improvements

- **Shadows**: `DropShadowEffect` pro depth
- **Rounded corners**: `CornerRadius="8"`
- **Modern colors**: Blue (#3B82F6), Gray (#64748B)
- **Typography**: Clear hierarchy, proper sizing
- **Spacing**: Consistent margins and padding
- **States**: Loading, Error, Success feedback

## 📦 NuGet Packages

```xml
<PackageReference Include="Autofac" Version="8.1.0" />
<PackageReference Include="Autofac.Extensions.DependencyInjection" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.11" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
```

## 🔑 Key Takeaways

1. **Navigator > WindowManager** - Jednodušší API, typová bezpečnost
2. **IDbContextFactory > Scoped DbContext** - Lepší izolace
3. **CQRS** - Čistá separace logiky
4. **ViewModel First** - Flexibilnější než View first
5. **Dialogy pro editace** - Lepší UX než non-modal
6. **CommunityToolkit.Mvvm** - Méně boilerplate
7. **Autofac + MS.DI** - Best of both worlds

## 📝 TODO (Optional enhancements)

- [ ] Implementovat Customer CQRS handlers
- [ ] Implementovat Order CQRS handlers
- [ ] Validation ve ViewModelech (FluentValidation?)
- [ ] Unit testy pro Handlers
- [ ] Error boundaries / global exception handling
- [ ] Workflow s Navigator.NavigateTo()
- [ ] Async initialization pattern
