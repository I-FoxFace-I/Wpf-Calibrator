using Calibrator.WpfControl.Controls.UniForm.Models;
using Calibrator.WpfControl.Sandbox.ViewModels;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calibrator.WpfControl.Sandbox.Views;

/// <summary>
/// UniForm demo user control view.
/// </summary>
public partial class UniFormDemo : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UniFormDemo"/> class.
    /// </summary>
    public UniFormDemo()
    {
        this.InitializeComponent();
        this.DataContext = new UniFormDemoViewModel();
    }
}
