using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.UniForm.Models;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;

namespace Calibrator.WpfControl.Sandbox.Models;

/// <summary>
/// Transducer model.
/// </summary>
public class Transducer : EquipmentBase
{
    private string? _inputRange;
    private string? _outputSignal;

    public string? InputRange
    {
        get => _inputRange;
        set { _inputRange = value; OnPropertyChanged(); }
    }

    public string? OutputSignal
    {
        get => _outputSignal;
        set { _outputSignal = value; OnPropertyChanged(); }
    }
}