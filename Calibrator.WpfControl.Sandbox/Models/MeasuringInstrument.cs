using Calibrator.WpfControl.Controls.UniForm.Models;
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

namespace Calibrator.WpfControl.Sandbox.Models;

public class MeasuringInstrument : EquipmentBase
{
    private string _measurementType;
    private double? _accuracy;
    private DateTime? _calibrationDate;

    public string MeasurementType
    {
        get => _measurementType;
        set { _measurementType = value; OnPropertyChanged(); }
    }

    public double? Accuracy
    {
        get => _accuracy;
        set { _accuracy = value; OnPropertyChanged(); }
    }

    public DateTime? CalibrationDate
    {
        get => _calibrationDate;
        set { _calibrationDate = value; OnPropertyChanged(); }
    }
}
