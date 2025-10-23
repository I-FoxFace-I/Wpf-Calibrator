using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Calibrator.WpfApplication.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Window> _openDialogs = new();

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Open<TViewModel, TParameter>(TParameter parameter) where TViewModel : class
    {
        var viewModelType = typeof(TViewModel);
        
        // Close existing dialog if open
        if (_openDialogs.ContainsKey(viewModelType))
        {
            _openDialogs[viewModelType].Close();
            _openDialogs.Remove(viewModelType);
        }

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        if (viewModel is IDialogViewModel<TParameter> dialogViewModel)
        {
            dialogViewModel.Parameter = parameter;
        }

        // Find the corresponding view
        var viewTypeName = viewModelType.Name.Replace("ViewModel", "View");
        var viewType = viewModelType.Assembly.GetType($"Calibrator.WpfApplication.Views.Dialogs.{viewTypeName}");
        
        if (viewType == null)
        {
            throw new InvalidOperationException($"View type {viewTypeName} not found");
        }

        var view = (Window)Activator.CreateInstance(viewType)!;
        view.DataContext = viewModel;

        _openDialogs[viewModelType] = view;
        
        view.Closed += (_, _) =>
        {
            _openDialogs.Remove(viewModelType);
        };

        view.ShowDialog();
    }

    public void Close<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        var viewModelType = typeof(TViewModel);
        
        if (_openDialogs.ContainsKey(viewModelType))
        {
            _openDialogs[viewModelType].Close();
            _openDialogs.Remove(viewModelType);
        }
    }
}


