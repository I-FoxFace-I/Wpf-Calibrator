using Calibrator.WpfApplication.ViewModels;
using Calibrator.WpfApplication.Views.Base;

namespace Calibrator.WpfApplication.Views;

public partial class ControllersOverviewView : BaseView
{
    public ControllersOverviewView(ControllersOverviewViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        
        Loaded += async (_, _) =>
        {
            await viewModel.InitializeAsync();
        };
    }
}

