using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Calibrator.WpfControl.Sandbox.ViewModels;

namespace Calibrator.WpfControl.Sandbox.Views;

public partial class DropdownDemo : UserControl
{
    public DropdownDemo()
    {
        InitializeComponent();
        DataContext = new DropdownDemoViewModel();
    }
}

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
}
