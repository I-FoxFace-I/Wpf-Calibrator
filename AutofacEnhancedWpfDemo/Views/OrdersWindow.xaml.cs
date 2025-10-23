using Autofac;
using AutofacEnhancedWpfDemo.ViewModels;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views;


// ========== OrdersWindow ==========
public partial class OrdersWindow : ScopedWindow
{
    public OrdersWindow(
        ILifetimeScope parentScope,
        ILogger<OrdersWindow> logger)
        : base(parentScope, logger, "orders")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is OrdersViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

///// <summary>
///// Converter to show/hide content based on workflow step
///// </summary>
//public class StepVisibilityConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
//        {
//            return currentStep == step ? Visibility.Visible : Visibility.Collapsed;
//        }
//        return Visibility.Collapsed;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

///// <summary>
///// Converter for step progress indicator colors
///// </summary>
//public class StepColorConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
//        {
//            if (currentStep > step)
//                return Colors.Green; // Completed
//            if (currentStep == step)
//                return Color.FromRgb(59, 130, 246); // Active (Blue)
//            return Color.FromRgb(203, 213, 225); // Pending (Gray)
//        }
//        return Colors.Gray;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}
