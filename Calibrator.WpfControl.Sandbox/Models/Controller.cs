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

public class Controller : EquipmentBase
{
    private string _controllerType;
    private string _firmwareVersion;

    public string ControllerType
    {
        get => _controllerType;
        set { _controllerType = value; OnPropertyChanged(); }
    }

    public string FirmwareVersion
    {
        get => _firmwareVersion;
        set { _firmwareVersion = value; OnPropertyChanged(); }
    }
}
