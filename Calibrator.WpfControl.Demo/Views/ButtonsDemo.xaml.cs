using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calibrator.WpfControl.Demo.Views;

public partial class ButtonsDemo : UserControl
{
    public ButtonsDemo()
    {
        InitializeComponent();
        DataContext = new ButtonsDemoViewModel();
    }
}

public class ButtonsDemoViewModel
{
    public ICommand ButtonClickCommand { get; }

    public ButtonsDemoViewModel()
    {
        ButtonClickCommand = new RelayCommand(OnButtonClick, () => true);
    }

    private void OnButtonClick()
    {
        MessageBox.Show("Button clicked!", "Demo", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

// Simple RelayCommand implementation for demo
//public class RelayCommand : ICommand
//{
//    private readonly Action _execute;
//    private readonly Func<bool> _canExecute;

//    public RelayCommand(Action execute, Func<bool> canExecute)
//    {
//        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//        _canExecute = canExecute ?? throw new ArgumentException(nameof(canExecute));
//    }

//    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

//    public void Execute(object parameter) => _execute();

//    public event EventHandler CanExecuteChanged
//    {
//        add => CommandManager.RequerySuggested += value;
//        remove => CommandManager.RequerySuggested -= value;
//    }
//}
