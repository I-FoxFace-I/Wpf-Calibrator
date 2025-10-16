using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Controls.UniTable.Models;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.Models;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}
