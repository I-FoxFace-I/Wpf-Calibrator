using System.Collections.Generic;
using System.Windows.Controls;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;

namespace Calibrator.WpfControl.Sandbox.Views;

public partial class NumericDemo : UserControl
{
    public NumericDemo()
    {
        InitializeComponent();
        SetupValidators();
    }

    private void SetupValidators()
    {
        ValidatedNumeric.Validators = new List<IValidator<object>>
        {
            new RequiredValidator("Temperature is required"),
            new RangeValidator(-50, 50, "Temperature must be between -50°C and 50°C")
        };
    }
}
