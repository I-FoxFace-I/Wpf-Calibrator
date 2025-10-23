# Calibrator.WpfApplication - Service Calibrator WPF Demo

This is a standalone WPF application extracted from the NewCalibrator module of the original ServiceCalibrator project. It demonstrates modern WPF development patterns with custom UI components and clean architecture.

## 🚀 Features

- **Custom UI Components**: Reusable ScButton, ScTextBox, ScDropdown, and UniTable components
- **MVVM Pattern**: Clean separation between Views and ViewModels
- **Modern WPF**: Uses .NET 8 with nullable reference types
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Mock Data**: Sample controllers and equipment templates for demonstration

## 🏗️ Architecture

### Project Structure

```
Calibrator.WpfApplication/
├── Views/
│   ├── Components/          # Reusable UI components
│   │   ├── ScButton/        # Custom button with icons
│   │   ├── ScTextBox/       # Custom text input
│   │   ├── ScDropdown/      # Custom dropdown
│   │   └── UniTable/        # Universal data grid
│   ├── Dialogs/             # Modal dialogs
│   └── Base/                # Base classes for views
├── ViewModels/              # MVVM ViewModels
├── Models/                  # Data models and DTOs
├── Services/                # Business services and repositories
├── Converters/              # WPF value converters
└── Resources/               # Styles, templates, fonts
```

### Design Patterns

- **MVVM (Model-View-ViewModel)**: Clean separation of concerns
- **Component-based UI**: Reusable, configurable components
- **Dependency Injection**: Loose coupling and testability
- **Repository Pattern**: Mock repositories for data access

## 📦 Key Dependencies

### UI Libraries

- **Telerik UI for WPF** (`Telerik.UI.for.Wpf.AllControls`)
  - Professional WPF control suite
  - Used for: RadGridView (data tables), RadComboBox (dropdowns), RadBusyIndicator (loading)
  - **Note**: Requires Telerik license for commercial use
  - **Alternative**: Can be replaced with standard WPF controls or DevExpress

- **MahApps.Metro.IconPacks** (`MahApps.Metro.IconPacks.Material`)
  - Material Design icons for WPF
  - Used throughout custom components for consistent iconography
  - **Free and open source**

### MVVM Framework

- **CommunityToolkit.Mvvm** (`CommunityToolkit.Mvvm`)
  - Modern MVVM framework (successor to MVVM Light)
  - Provides: ObservableObject, RelayCommand, ObservableProperty attributes
  - **Free and officially supported by Microsoft**

### Behaviors

- **Microsoft.Xaml.Behaviors.Wpf** (`Microsoft.Xaml.Behaviors.Wpf`)
  - Behaviors and interactions for WPF
  - Used for advanced UI interactions
  - **Free and officially supported by Microsoft**

## 🎨 Custom Components

### ScButton
- Custom button with icon support
- Three styles: Regular, Accent, Transparent
- Configurable size and appearance

### ScTextBox
- Text input with integrated label
- Watermark support via Telerik RadWatermarkTextBox
- Consistent styling with shadow effects

### ScDropdown
- Dropdown selection with label
- Based on Telerik RadComboBox
- Support for DisplayMemberPath and data binding

### UniTable
- Universal data grid component
- Dynamic column generation from expressions
- Integrated action buttons (Edit, Delete, etc.)
- Based on Telerik RadGridView

## 🔧 Loading Management

Unlike the original project that used PostSharp/Metalama for AOP, this version implements manual loading state management:

```csharp
// Original (with [WithLoading] attribute)
[WithLoading]
private async Task LoadData()
{
    // Business logic
}

// New approach (manual loading management)
private async Task LoadData()
{
    await ExecuteWithLoading(async () =>
    {
        // Business logic
    });
}
```

Benefits of manual approach:
- No external AOP dependencies
- Explicit control over loading states
- Easier debugging and testing
- Better performance (no IL manipulation)

## 🎯 Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or JetBrains Rider
- **Telerik UI for WPF license** (for full functionality)

### Building and Running

1. Clone or copy the Calibrator.WpfApplication folder
2. Open `Calibrator.WpfApplication.csproj` in your IDE
3. Restore NuGet packages
4. Build and run the application

### Without Telerik License

If you don't have a Telerik license, you can:

1. **Replace Telerik controls** with standard WPF controls:
   - `RadGridView` → `DataGrid`
   - `RadComboBox` → `ComboBox`
   - `RadWatermarkTextBox` → `TextBox`
   - `RadBusyIndicator` → Custom loading overlay

2. **Use free alternatives**:
   - Material Design In XAML Toolkit
   - Modern WPF UI
   - HandyControls

## 🧪 Demo Data

The application includes mock repositories with sample data:
- Controllers (PowerFocus 6000, PowerFocus 4000)
- Equipment Templates (Torque Wrench, Screwdriver)
- Calibration records (50 sample entries)

## 🛠️ Extending the Application

### Adding New Entities

1. Create model in `Models/Entities.cs`
2. Add DTO in `Models/Dtos.cs`
3. Create repository interface and mock implementation
4. Add ViewModels for overview and edit operations
5. Create corresponding Views
6. Register in DI container

### Customizing Components

All Sc* components are highly customizable through dependency properties:

```xaml
<scButton:ScButtonComponent 
    Content="Custom Button"
    ButtonType="Accent"
    IconKind="Save"
    ButtonWidth="150"
    Command="{Binding SaveCommand}" />
```

## 📄 License

This demo project is based on the ServiceCalibrator application. Please ensure you have appropriate licenses for:
- Telerik UI for WPF (commercial use)
- Any other commercial dependencies

All custom code and components can be freely modified and extended for your needs.

## 🤝 Contributing

This is a demo/template project. Feel free to:
- Extend functionality
- Replace Telerik with free alternatives
- Add new custom components
- Improve the architecture

## 📞 Support

For questions about:
- **Telerik controls**: Check Telerik documentation
- **MVVM patterns**: CommunityToolkit.Mvvm documentation
- **WPF best practices**: Microsoft WPF documentation


