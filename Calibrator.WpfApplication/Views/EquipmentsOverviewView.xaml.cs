using Calibrator.WpfApplication.ViewModels;
using Calibrator.WpfApplication.Views.Base;

namespace Calibrator.WpfApplication.Views;

public partial class EquipmentsOverviewView : BaseView
{
    public EquipmentsOverviewView(EquipmentsOverviewViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            await viewModel.InitializeAsync();
        };
    }
}

