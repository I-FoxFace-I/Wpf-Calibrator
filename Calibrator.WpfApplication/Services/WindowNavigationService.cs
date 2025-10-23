using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Calibrator.WpfApplication.Services;

public class WindowNavigationService : IWindowNavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public WindowNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : IWindowNavigatableViewModel
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        // Assuming naming convention: ViewModel name without "ViewModel" suffix is the View name
        var viewTypeName = typeof(TViewModel).FullName?.Replace("ViewModel", "View")
                              .Replace(".ViewModels.", ".Views.");
        
        if (viewTypeName == null)
        {
            throw new InvalidOperationException($"Cannot determine view type for {typeof(TViewModel).Name}");
        }

        var viewType = Type.GetType(viewTypeName);
        if (viewType == null)
        {
            // Try to find in current assembly
            viewType = typeof(TViewModel).Assembly.GetType(viewTypeName);
        }
        
        if (viewType == null)
        {
            throw new InvalidOperationException($"Cannot find view type: {viewTypeName}");
        }

        var view = Activator.CreateInstance(viewType) as Window;
        if (view == null)
        {
            throw new InvalidOperationException($"Cannot create instance of view: {viewTypeName}");
        }

        view.DataContext = viewModel;
        
        // Close all other windows and show the new one
        foreach (Window window in Application.Current.Windows)
        {
            if (window != view)
            {
                window.Close();
            }
        }
        
        view.Show();
    }
}
