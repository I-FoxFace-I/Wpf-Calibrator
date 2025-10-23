using Calibrator.WpfApplication.ViewModels;
using Calibrator.WpfApplication.Views.Base;

namespace Calibrator.WpfApplication.Views.Dialogs;

public partial class EditEquipmentTemplateDialogView : BaseView
{
    public EditEquipmentTemplateDialogView()
    {
        InitializeComponent();
        
        Loaded += async (_, _) =>
        {
            if (DataContext is EditEquipmentTemplateDialogViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        };
    }
}


