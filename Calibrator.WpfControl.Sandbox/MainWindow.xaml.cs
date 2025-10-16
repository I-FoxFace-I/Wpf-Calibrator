using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Calibrator.WpfControl.Sandbox.Commands;
using Calibrator.WpfControl.Sandbox.Views;

namespace Calibrator.WpfControl.Sandbox;

/// <summary>
/// MainWindow View of the DEMO app.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        this.InitializeComponent();
        this.DataContext = this;
        this.ShowTextBoxDemo(); // Show TextBox demo by default
    }

    public ICommand ShowButtonsDemoCommand => new RelayCommand(() => ShowButtonsDemo());
    public ICommand ShowTextBoxDemoCommand => new RelayCommand(() => ShowTextBoxDemo());
    public ICommand ShowTextBlockDemoCommand => new RelayCommand(() => ShowTextBlockDemo());
    public ICommand ShowNumericDemoCommand => new RelayCommand(() => ShowNumericDemo());
    public ICommand ShowDropdownDemoCommand => new RelayCommand(() => ShowDropdownDemo());
    public ICommand ShowLoadingCommandDemoCommand => new RelayCommand(() => ShowLoadingCommandDemo());
    public ICommand ShowDataLoadingDemoCommand => new RelayCommand(() => ShowDataLoadingDemo());
    public ICommand ShowUniTableDemoCommand => new RelayCommand(() => ShowUniTableDemo());
    public ICommand ShowSmartTableDemoCommand => new RelayCommand(() => ShowSmartTableDemo());
    public ICommand ShowUniFormDemoCommand => new RelayCommand(() => ShowUniFormDemo());
    public ICommand ShowSmartContainerDemoCommand => new RelayCommand(() => ShowSmartContainerDemo());

    private void ShowButtonsDemo()
    {
        this.ContentArea.Content = new ButtonsDemo();
    }

    private void ShowTextBoxDemo()
    {
        this.ContentArea.Content = new TextBoxDemo();
    }

    private void ShowTextBlockDemo()
    {
        this.ContentArea.Content = new TextBlockDemo();
    }

    private void ShowNumericDemo()
    {
        this.ContentArea.Content = new NumericDemo();
    }

    private void ShowDropdownDemo()
    {
        this.ContentArea.Content = new DropdownDemo();
    }

    private void ShowDataLoadingDemo()
    {
        this.ContentArea.Content = new DataLoadingDemo();
    }

    private void ShowUniTableDemo()
    {
        this.ContentArea.Content = new UniTableDemo();
    }

    private void ShowSmartTableDemo()
    {
        this.ContentArea.Content = new SmartTableDemo();
    }

    private void ShowUniFormDemo()
    {
        this.ContentArea.Content = new UniFormDemo();
    }

    private void ShowSmartContainerDemo()
    {
        this.ContentArea.Content = new SmartContainerDemo();
    }
}
