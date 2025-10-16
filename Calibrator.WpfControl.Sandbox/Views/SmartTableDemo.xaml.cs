using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.ScSmartTable;
using Calibrator.WpfControl.Controls.ScSmartTable.Models;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Sandbox.ViewModels;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.Views;

public partial class SmartTableDemo : UserControl
{
    public SmartTableDemo()
    {
        InitializeComponent();
        DataContext = new SmartTableDemoViewModel(SmartTable);
    }
}
