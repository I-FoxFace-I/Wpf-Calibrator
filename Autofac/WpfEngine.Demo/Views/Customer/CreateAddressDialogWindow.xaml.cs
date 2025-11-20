using System;
using System.Windows;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Enums;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.Views.Dialogs;

/// <summary>
/// Dialog window for creating new address
/// Implements IScopedView and IDialogView for proper integration
/// </summary>
public partial class CreateAddressDialogWindow : ScopedDialogWindow
{
    public CreateAddressDialogWindow(ILogger<CreateAddressDialogWindow> logger) : base(logger)
    {
        InitializeComponent();

        // Generate unique window ID
        //AssignedWindowId = Guid.NewGuid();
        
        Logger.LogInformation("[CREATE_ADDRESS_DIALOG_WINDOW] Window created with ID: {WindowId}", 
            AssignedWindowId);

        // Subscribe to window events
        base.Loaded += OnLoaded;
        base.Closed += OnClosed;
    }

    // ========== IScopedView Implementation ==========

    //public Guid AssignedWindowId { get; }

    //// ========== IDialogView Implementation ==========

    //public Guid WindowId => AssignedWindowId;
    
    public override DialogType DialogType => DialogType.Custom;
    
    public override string? AppModule => "Demo";

    // ========== Event Handlers ==========

    protected void OnLoaded(object sender, RoutedEventArgs e)
    {
        Logger.LogInformation("[CREATE_ADDRESS_DIALOG_WINDOW] Window loaded");
    }

    protected void OnClosed(object? sender, EventArgs e)
    {
        Logger.LogInformation("[CREATE_ADDRESS_DIALOG_WINDOW] Window closed with result: {DialogResult}", 
            base.DialogResult);
        
        // Cleanup
        base.Loaded -= OnLoaded;
        base.Closed -= OnClosed;
    }
}
