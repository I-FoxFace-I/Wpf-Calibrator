using System.Collections.Generic;
using System.Windows.Controls;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;

namespace Calibrator.WpfControl.Sandbox.Views;

public partial class TextBoxDemo : UserControl
{
    public TextBoxDemo()
    {
        InitializeComponent();
        SetupValidators();
    }

    private void SetupValidators()
    {
        // Required validator
        RequiredTextBox.Validators = new List<IValidator<object>>
        {
            new RequiredValidator("Email is required")
        };

        // Min length validator
        MinLengthTextBox.Validators = new List<IValidator<object>>
        {
            new RequiredValidator("Username is required"),
            new MinLengthValidator(5, "Username must be at least 5 characters")
        };

        // Email regex validator
        EmailTextBox.Validators = new List<IValidator<object>>
        {
            new RequiredValidator("Email is required"),
            new RegexValidator(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", "Invalid email format")
        };
    }
}
