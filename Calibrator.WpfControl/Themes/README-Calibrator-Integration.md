# Calibrator.WpfControl – Quick Integration

## 1) Merge a single umbrella dictionary
In your `App.xaml`:

```xml
<Application ...>
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Calibrator.WpfControl;component/Themes/Calibrator.All.xaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

> If your app **does not use Telerik UI**, remove the UniTable/Dropdown style dictionaries from `Calibrator.All.xaml` or split them into a separate `Calibrator.Telerik.xaml` (recommended).

## 2) Use components
```xml
<Window ...
        xmlns:calib="clr-namespace:Calibrator.WpfControl.Controls.ScButton;assembly=Calibrator.WpfControl">
  <calib:ScButtonComponent Content="Click me" />
</Window>
```

## 3) Optional: multi-target the library
In `Calibrator.WpfControl.csproj`:

```xml
<PropertyGroup>
  <TargetFrameworks>net8.0-windows;net9.0-windows</TargetFrameworks>
  <UseWPF>true</UseWPF>
</PropertyGroup>
```

## 4) ThemeInfo attribute (optional for UserControls, safe to add)
Create `Properties/AssemblyInfo.cs`:

```csharp
using System.Windows;

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
```
