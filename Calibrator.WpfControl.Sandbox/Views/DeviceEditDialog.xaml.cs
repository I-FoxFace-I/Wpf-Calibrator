using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.UniForm.Models;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Sandbox.Models;
using Calibrator.WpfControl.Sandbox.ViewModels;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.Views;

// ============================================
// Device Edit Dialog
// ============================================
public partial class DeviceEditDialog : Window
{
    public IndustrialDevice UpdatedDevice { get; private set; }

    public DeviceEditDialog(IndustrialDevice device)
    {
        InitializeComponent();
        
        var viewModel = new DeviceEditViewModel(device);
        viewModel.SaveRequested += (updatedDevice) =>
        {
            UpdatedDevice = updatedDevice;
            DialogResult = true;
            Close();
        };
        viewModel.CancelRequested += () =>
        {
            DialogResult = false;
            Close();
        };

        DataContext = viewModel;
        UpdatedDevice = device;
    }
}

